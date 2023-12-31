""" Authorize a user to access to an event based specific time bound event."""

import logging
import string
from uuid import UUID

import asyncpg
from fastapi import HTTPException

# pylint: disable=E0402
from .lru_cache_with_expiry import lru_cache_with_expiry
from .monitor import Monitor, MonitorEntity

MAX_AUTH_TOKEN_LENGTH = 40

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

    async def __is_user_authorized(
        self, event_code: str, api_key: UUID, deployment_name: str
    ) -> AuthorizeResponse:
        """Check if user is authorized"""

        try:
            conn = await self.db_manager.get_connection()

            result = await conn.fetchrow(
                "SELECT * from aoai.get_attendee_authorized($1, $2)", event_code, api_key
            )

            if result is None or len(result) == 0:
                raise HTTPException(
                    status_code=401,
                    detail="Authentication failed.",
                )

            result_dict = dict(result)
            result_dict["is_authorized"] = True
            result_dict["deployment_name"] = deployment_name

            return AuthorizeResponse(**result_dict)

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

        event_code = id_parts[0].strip()
        api_key = id_parts[1].strip()

        if not 4 <= len(event_code) <= MAX_AUTH_TOKEN_LENGTH:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        # is the user_id a valid guid
        try:
            api_key = UUID(api_key)
        except ValueError as exc:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            ) from exc

        # check event code is only printable characters
        if not all(c in string.printable for c in event_code):
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        authorize_response = await self.__is_user_authorized(
            event_code=event_code, api_key=api_key, deployment_name=deployment_name
        )

        return authorize_response

    async def authorize_azure_api_access(
        self, *, headers: str, deployment_name: str
    ) -> AuthorizeResponse:
        """authorize api access"""

        if "api-key" not in headers:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        access_token = headers.get("api-key")

        return await self.__authorize(access_token=access_token, deployment_name=deployment_name)
