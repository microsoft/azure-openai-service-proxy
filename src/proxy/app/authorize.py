""" Authorize a user to access to an event based specific time bound event."""

import logging
import string
from uuid import UUID

import asyncpg
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

    def __init__(self, *, connection_string: str, db_manager) -> None:
        self.connection_string = connection_string
        self.db_manager = db_manager
        self.monitor = Monitor(connection_string=connection_string)
        self.logging = logging.getLogger(__name__)

    async def __is_user_authorized(self, event_id: str, api_key: UUID, deployment_name: str) -> AuthorizeResponse:
        """Check if user is authorized"""

        try:
            conn = await self.db_manager.get_connection()

            result = await conn.fetchrow("SELECT * from aoai.get_attendee_authorized($1, $2)", event_id, api_key)

            if result is None or len(result) == 0:
                raise HTTPException(
                    status_code=401,
                    detail="Authentication failed.",
                )

            authorize_response = AuthorizeResponse(
                is_authorized=True,
                max_token_cap=result.get("max_token_cap"),
                daily_request_cap=result.get("daily_request_cap"),
                user_id=result.get("user_id"),
                event_id=result.get("event_id"),
                event_code=result.get("event_code"),
                user_token=api_key,
                event_name=result.get("event_code"),
                event_url=result.get("event_url"),
                event_url_text=result.get("event_url_text"),
                organizer_name=result.get("organizer_name"),
                organizer_email=result.get("organizer_email"),
                deployment_name=deployment_name,
            )

            return authorize_response

        except HTTPException:
            raise

        except asyncpg.exceptions.PostgresError as error:
            self.logging.error("Postgres error: %s", str(error))
            raise HTTPException(
                status_code=503,
                detail="Error reading model catalog.",
            ) from error

        except Exception as exception:
            self.logging.error("General exception in user_authorized: %s", str(exception))
            raise HTTPException(
                status_code=401,
                detail="Authentication failed. General exception.",
            ) from exception

    @lru_cache_with_expiry(maxsize=128, ttl=300)
    async def __authorize(self, *, access_token: str, deployment_name: str) -> AuthorizeResponse:
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
            event_id=event_id, api_key=user_id, deployment_name=deployment_name
        )

        return authorize_response

    async def __authorize_azure_api_access(self, *, headers, deployment_name: str) -> AuthorizeResponse:
        """validate azure sdk formatted API request"""

        if "api-key" not in headers:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        access_token = headers.get("api-key")

        return await self.__authorize(access_token=access_token, deployment_name=deployment_name)

    async def authorize_api_access(self, *, headers: str, deployment_name: str) -> AuthorizeResponse:
        """authorize api access"""

        return await self.__authorize_azure_api_access(headers=headers, deployment_name=deployment_name)
