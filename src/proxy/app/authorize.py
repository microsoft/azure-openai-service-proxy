""" Authorize a user to access to an event based specific time bound event."""

import logging
from uuid import UUID

import asyncpg
from fastapi import HTTPException

# pylint: disable=E0402
from .lru_cache_with_expiry import lru_cache_with_expiry
from .monitor import MonitorEntity

MAX_AUTH_TOKEN_LENGTH = 40

logging.basicConfig(level=logging.WARNING)


class AuthorizeResponse(MonitorEntity):
    """Response object for Authorize class."""


class Authorize:
    """Authorizes a user to access a specific time bound event."""

    def __init__(self, *, db_manager) -> None:
        self.db_manager = db_manager
        self.logging = logging.getLogger(__name__)

    async def __is_user_authorized(self, api_key: UUID, deployment_name: str) -> AuthorizeResponse:
        """Check if user is authorized"""

        try:
            conn = await self.db_manager.get_connection()

            result = await conn.fetchrow("SELECT * from aoai.get_attendee_authorized($1)", api_key)

            if result is None or len(result) == 0:
                raise HTTPException(
                    status_code=401,
                    detail="Authentication failed.",
                )

            if result.get("rate_limit_exceed"):
                raise HTTPException(
                    status_code=429,
                    detail={
                        "error": {
                            "code": "429",
                            "message": "Daily request rate exceeded",
                        }
                    },
                )

            result_dict = dict(result)
            result_dict["is_authorized"] = True
            result_dict["deployment_name"] = deployment_name
            result_dict["api_key"] = api_key
            del result_dict["rate_limit_exceed"]

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

    @lru_cache_with_expiry(maxsize=128, ttl=180)
    async def __authorize(self, *, access_token: str, deployment_name: str) -> AuthorizeResponse:
        """Authorizes a user to access a specific time bound event."""

        # is the user_id a valid guid
        try:
            api_key = UUID(access_token)
        except ValueError as exc:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            ) from exc

        authorize_response = await self.__is_user_authorized(
            api_key=api_key, deployment_name=deployment_name
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
