""" Authorize a user to access to an event based specific time bound event."""

import logging
from uuid import UUID

import asyncpg
from fastapi import HTTPException

# pylint: disable=E0402
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

        pool = await self.db_manager.get_connection()

        try:
            async with pool.acquire() as conn:
                result = await conn.fetchrow(
                    "SELECT * from aoai.get_attendee_authorized($1)", api_key
                )

            if result is None or len(result) == 0:
                raise HTTPException(
                    status_code=401,
                    detail="Authentication failed.",
                )

            if result.get("rate_limit_exceed"):
                raise HTTPException(
                    status_code=429,
                    detail=(
                        f"The event daily request rate of {result.get('daily_request_cap')} "
                        "calls to has been exceeded. Requests are disabled until UTC midnight."
                    ),
                )

            result_dict = dict(result)
            result_dict["is_authorized"] = True
            result_dict["deployment_name"] = deployment_name
            del result_dict["rate_limit_exceed"]

            return AuthorizeResponse(**result_dict)

        except HTTPException:
            raise

        except asyncpg.exceptions.PostgresError as error:
            self.logging.error("Postgres error in __is_user_authorized: %s", str(error))
            raise HTTPException(
                status_code=503,
                detail="Authentication failed.",
            ) from error

        except Exception as exception:
            self.logging.error("General exception in __is_user_authorized: %s", str(exception))
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            ) from exception

    # @lru_cache_with_expiry(maxsize=128, ttl=180)
    async def __authorize(self, *, api_key: str, deployment_name: str) -> AuthorizeResponse:
        """Authorizes a user to access a specific time bound event."""

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

        api_key = headers.get("api-key")

        return await self.__authorize(api_key=api_key, deployment_name=deployment_name)
