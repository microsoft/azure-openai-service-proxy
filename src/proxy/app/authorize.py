""" Authorize a user to access to an event based specific time bound event."""

import logging
import string
from uuid import UUID

import pyodbc
from fastapi import HTTPException

# pylint: disable=E0402
from .lru_cache_with_expiry import lru_cache_with_expiry
from .monitor import Monitor, MonitorEntity

USER_TABLE_NAME = "user"
EVENT_TABLE_NAME = "event"

MAX_AUTH_TOKEN_LENGTH = 40
MANAGEMENT_TABLE_NAME = "management"

logging.basicConfig(level=logging.WARNING)


class AuthorizeResponse(MonitorEntity):
    """Response object for Authorize class."""


class Authorize:
    """Authorizes a user to access a specific time bound event."""

    def __init__(self, *, connection_string: str, sql_conn: pyodbc.Connection) -> None:
        self.connection_string = connection_string
        self.sql_conn = sql_conn
        self.monitor = Monitor(connection_string=connection_string)
        self.logging = logging.getLogger(__name__)

    async def __is_user_authorized(self, event_id: str, api_key: UUID, request_class: str) -> AuthorizeResponse:
        """Check if user is authorized"""

        try:
            cursor = self.sql_conn.cursor()
            cursor.execute("{CALL dbo.EventAttendeeAuthorized(?, ?)}", (event_id, api_key))
            result = cursor.fetchone()

            if len(result) == 0:
                raise HTTPException(
                    status_code=401,
                    detail="Authentication failed.",
                )

            authorize_response = AuthorizeResponse(
                is_authorized=True,
                max_token_cap=result.MaxTokenCap,
                daily_request_cap=result.DailyRequestCap,
                entra_id=result.EntraID,
                event_id=result.EventID,
                event_code=result.EventCode,
                user_token=api_key,
                event_name=result.EventCode,
                event_url=result.EventUrl,
                event_url_text=result.EventUrlText,
                organizer_name=result.OrganizerName,
                organizer_email=result.OrganizerEmail,
                request_class=request_class,
            )

            return authorize_response

        except pyodbc.Error as error:
            self.logging.error("pyodbc error: %s", str(error))
            raise HTTPException(
                status_code=401,
                detail="Authentication failed. pyodbc error.",
            ) from error

        except Exception as exception:
            self.logging.error("General exception in user_authorized: %s", str(exception))
            raise HTTPException(
                status_code=401,
                detail="Authentication failed. General exception.",
            ) from exception

    @lru_cache_with_expiry(maxsize=128, ttl=300)
    async def __authorize(self, *, access_token: str, request_class: str) -> AuthorizeResponse:
        """Authorizes a user to access a specific time bound event."""

        id_parts = access_token.split("/")

        if len(id_parts) != 2:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        event_id = id_parts[0].strip()
        user_id = id_parts[1].strip()

        # throw a not Authentication failed exception if
        # the event_code len is less than 6 or greater than MAX_AUTH_TOKEN_LENGTH

        if not 6 <= len(event_id) <= MAX_AUTH_TOKEN_LENGTH:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        # is the user_id a valid guid
        try:
            user_id = UUID(user_id)
        except ValueError as exc:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            ) from exc

        # check event code does not user any of the azure table reserved characters for row key
        if any(c in event_id for c in ["\\", "/", "#", "?", "\t", "\n", "\r"]):
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        # check event code is only printable characters
        if not all(c in string.printable for c in event_id):
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        authorize_response = await self.__is_user_authorized(
            event_id=event_id, api_key=user_id, request_class=request_class
        )

        return authorize_response

    async def __authorize_azure_api_access(self, *, headers, request_class) -> AuthorizeResponse:
        """validate azure sdk formatted API request"""

        if "api-key" not in headers:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        access_token = headers.get("api-key")

        return await self.__authorize(access_token=access_token, request_class=request_class)

    async def __authorize_openai_api_access(self, *, headers, request_class) -> AuthorizeResponse:
        """validate openai sdk formatted API request"""

        if "Authorization" not in headers:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        auth_header = headers.get("Authorization")
        auth_parts = auth_header.split(" ")

        # check auth header is valid and has two parts and the first part is Bearer
        if len(auth_parts) != 2 or auth_parts[0] != "Bearer":
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        # the second part is the access token
        access_token = auth_parts[1]

        return await self.__authorize(access_token=access_token, request_class=request_class)

    async def authorize_api_access(self, *, headers: str, deployment_id: str, request_class: str) -> AuthorizeResponse:
        """authorize api access"""
        # if there is a deployment_id then this is an azure sdk request
        # otherwise it is an openai sdk request

        if deployment_id:
            return await self.__authorize_azure_api_access(headers=headers, request_class=request_class)

        return await self.__authorize_openai_api_access(headers=headers, request_class=request_class)
