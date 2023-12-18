""" Authorize a user to access to an event based specific time bound event."""

import logging
import string
from datetime import datetime
from uuid import UUID

import pytz
from azure.core.exceptions import AzureError
from azure.data.tables.aio import TableClient
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

    def __init__(self, *, connection_string: str) -> None:
        self.connection_string = connection_string
        self.monitor = Monitor(connection_string=connection_string)
        self.logging = logging.getLogger(__name__)

    async def __is_event_authorized(
        self, *, group_id: str, event_id: str, user_id: UUID, request_class: str
    ) -> AuthorizeResponse:
        """Check if event is authorized"""

        entity_found = False

        async with TableClient.from_connection_string(
            conn_str=self.connection_string, table_name=EVENT_TABLE_NAME
        ) as table_client:
            try:
                query_filter = f"PartitionKey eq '{group_id}' and " f"RowKey eq '{event_id}' and Active eq true"
                # get all columns from the table
                queried_entities = table_client.query_entities(
                    query_filter=query_filter,
                    select=[
                        "*",
                    ],
                )

                async for entity in queried_entities:
                    entity_found = True
                    event_friendly_name = entity.get("FriendlyName", "")
                    event_url = entity.get("EventUrl", "")
                    event_url_text = entity.get("EventUrlText", "")
                    organizer_name = entity.get("OrganizerName", "")
                    organizer_email = entity.get("OrganizerEmail", "")
                    start_utc = entity.get("StartUTC", pytz.utc.localize(datetime.max))
                    end_utc = entity.get("EndUTC", pytz.utc.localize(datetime.min))
                    max_token_cap = entity.get("MaxTokenCap", 512)

                if not entity_found:
                    raise HTTPException(
                        status_code=401,
                        detail="Authentication failed.",
                    )

                max_token_cap = max_token_cap if max_token_cap > 0 else 512

                current_time_utc = datetime.now(pytz.utc)

                is_authorized = start_utc <= current_time_utc <= end_utc

                if not is_authorized:
                    raise HTTPException(
                        status_code=401,
                        detail="Authentication failed.",
                    )

                authorize_response = AuthorizeResponse(
                    is_authorized=is_authorized,
                    max_token_cap=max_token_cap,
                    event_code=event_id,
                    user_token=user_id,
                    event_name=event_friendly_name,
                    event_url=event_url,
                    event_url_text=event_url_text,
                    organizer_name=organizer_name,
                    organizer_email=organizer_email,
                    request_class=request_class,
                    group_id=group_id,
                )

                self.monitor.log_api_call(entity=authorize_response)

                return authorize_response

            except AzureError as azure_error:
                self.logging.error(" AzureError: %s", str(azure_error))
                raise HTTPException(
                    status_code=401,
                    detail="Authentication failed. AzureError.",
                ) from azure_error

            except Exception as exception:
                self.logging.error("General exception in event_authorized: %s", str(exception))
                raise HTTPException(
                    status_code=401,
                    detail="Authentication failed. General exception.",
                ) from exception

    async def __is_user_authorized(self, event_id: str, user_id: UUID, request_class: str) -> AuthorizeResponse:
        """Check if user is authorized"""

        entity_found = False

        async with TableClient.from_connection_string(
            conn_str=self.connection_string, table_name=USER_TABLE_NAME
        ) as table_client:
            try:
                query_filter = f"PartitionKey eq '{event_id}' and " f"RowKey eq '{user_id}' and Active eq true"
                # get all columns from the table
                queried_entities = table_client.query_entities(
                    query_filter=query_filter,
                    select=[
                        "*",
                    ],
                )

                async for entity in queried_entities:
                    entity_found = True
                    group_id = entity.get("GroupID", None)

                if not entity_found:
                    raise HTTPException(
                        status_code=401,
                        detail="Authentication failed.",
                    )

                return await self.__is_event_authorized(
                    group_id=group_id,
                    event_id=event_id,
                    user_id=user_id,
                    request_class=request_class,
                )

            except AzureError as azure_error:
                self.logging.error(" AzureError: %s", str(azure_error))
                raise HTTPException(
                    status_code=401,
                    detail="Authentication failed. AzureError.",
                ) from azure_error

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
            event_id=event_id, user_id=user_id, request_class=request_class
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

    async def authorize_management_access(self, headers):
        """authorize management access"""

        if "Authorization" not in headers:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed.",
            )

        auth_header = headers.get("Authorization")
        auth_parts = auth_header.split(" ")

        if len(auth_parts) != 2 or auth_parts[0] != "Bearer":
            raise HTTPException(
                status_code=401,
                detail="Authentication failed. Invalid Token.",
            )

        guid = auth_parts[1]
        # check guid is valid by loading up as a guid
        try:
            guid = UUID(guid)
        except ValueError as exc:
            raise HTTPException(
                status_code=401,
                detail="Authentication failed. Invalid Token.",
            ) from exc

        async with TableClient.from_connection_string(
            conn_str=self.connection_string, table_name=MANAGEMENT_TABLE_NAME
        ) as table_client:
            try:
                query_filter = f"PartitionKey eq 'management' and " f"RowKey eq '{guid}' and Active eq true"
                # get all columns from the table
                queried_entities = table_client.query_entities(
                    query_filter=query_filter,
                    select=[
                        "*",
                    ],
                )

                management_uuid = None
                async for entity in queried_entities:
                    management_uuid = entity.get("RowKey", None)

                if management_uuid:
                    return management_uuid

                raise HTTPException(
                    status_code=401,
                    detail="Authentication failed.",
                )

            except HTTPException:
                raise

            except AzureError as azure_error:
                self.logging.error(
                    "Authentication failed. Azure Error: %s",
                    azure_error.message,
                )
                raise HTTPException(
                    status_code=401,
                    detail=azure_error.message,
                ) from azure_error

            except Exception as exception:
                self.logging.error("General exception in management authorize: %s", str(exception))
                raise HTTPException(
                    status_code=401,
                    detail="Authentication failed. General exception.",
                ) from exception
