""" Authorize a user to access playground based specific time bound event."""

import string
import logging
from datetime import datetime, timedelta
import pytz

from azure.data.tables import TableServiceClient
from azure.data.tables.aio import TableClient

from azure.core.exceptions import (
    HttpResponseError,
    ServiceRequestError,
    ClientAuthenticationError,
)

CACHE_EXPIRY_MINUTES = 10
PARTITION_KEY = "playground"
TABLE_NAME = "playgroundauthorization"


class AuthorizeResponse:
    """Response object for Authorize class."""

    def __init__(self, is_authorized: bool, max_token_cap: int) -> None:
        self.is_authorized = is_authorized
        self.max_token_cap = max_token_cap


class Authorize:
    """Authorizes a user to access a specific time bound event."""

    def __init__(self, connection_string) -> None:
        self.connection_string = connection_string
        self.event_cache = []
        self.cache_expiry = None

        logging.basicConfig(level=logging.WARNING)
        self.logger = logging.getLogger(__name__)

        # Create events playground authorization table if it does not exist
        try:
            table_service_client = TableServiceClient.from_connection_string(
                conn_str=self.connection_string
            )

            table_service_client.create_table_if_not_exists(table_name=TABLE_NAME)
        except Exception as exception:
            logging.error("General exception creating table: %s", str(exception))

    def __is_event_authorised_cached(self, event_code: str) -> AuthorizeResponse | None:
        """checks if event code is in the cache"""
        if self.cache_expiry:
            if datetime.now() > self.cache_expiry:
                self.event_cache = []
                self.cache_expiry = None
            else:
                # look for event_code in the self.event_cache list of dictionaries
                for event in self.event_cache:
                    if event_code in event:
                        start_utc = event.get(event_code).get("start_utc")
                        end_utc = event.get(event_code).get("end_utc")
                        max_token_cap = event.get(event_code).get("max_token_cap")
                        current_time_utc = datetime.now(pytz.utc)
                        return AuthorizeResponse(
                            is_authorized=start_utc <= current_time_utc <= end_utc,
                            max_token_cap=max_token_cap,
                        )

        return None

    async def __is_event_authorized(self, event_code: str) -> AuthorizeResponse | None:
        """checks if event code is in the cache, failing that get from table"""
        # get event code, start date and end time from table
        # add to cache dictionary object keyed by event code
        # check the current utc time is between start and end time

        async with TableClient.from_connection_string(
            conn_str=self.connection_string, table_name=TABLE_NAME
        ) as table_client:
            # check if event_code is in the cache
            authorised_response = self.__is_event_authorised_cached(event_code)
            if authorised_response is not None:
                return authorised_response

            try:
                name_filter = (
                    f"PartitionKey eq '{PARTITION_KEY}' and RowKey eq '{event_code}'"
                )
                queried_entities = table_client.query_entities(
                    query_filter=name_filter,
                    select=["StartUTC", "EndUTC", "Active", "MaxTokenCap"],
                )

                async for entity in queried_entities:
                    start_utc = entity["StartUTC"]
                    end_utc = entity["EndUTC"]
                    active = entity.get("Active", False)
                    max_token_cap = entity.get("MaxTokenCap", 1024)

                    if not active:
                        logging.warning("Event is not active: %s", event_code)
                        return None

                    current_time_utc = datetime.now(pytz.utc)

                    is_authorized = start_utc <= current_time_utc <= end_utc

                    if not is_authorized:
                        return None

                    # set cache_expiry to current time plus 10 minutes
                    if not self.cache_expiry:
                        self.cache_expiry = datetime.now() + timedelta(
                            minutes=CACHE_EXPIRY_MINUTES
                        )

                    self.event_cache.append(
                        {
                            event_code: {
                                "start_utc": start_utc,
                                "end_utc": end_utc,
                                "max_token_cap": max_token_cap,
                            }
                        }
                    )

                    return AuthorizeResponse(
                        is_authorized=is_authorized,
                        max_token_cap=max_token_cap,
                    )

            except ClientAuthenticationError as auth_error:
                logging.error("ClientAuthenticationError: %s", auth_error.message)
                return None

            except ServiceRequestError as service_request_error:
                logging.error("ServiceResponseError: %s", service_request_error.message)
                return None

            except HttpResponseError as response_error:
                logging.error("HttpResponseError: %s", response_error.message)
                return None

            except Exception as exception:
                logging.error(
                    "General exception in event_authorized: %s", str(exception)
                )
                return None

        return None

    async def authorize(self, event_code: str) -> AuthorizeResponse | None:
        """Authorizes a user to access a specific time bound event."""

        # check event_code is not empty
        if not event_code:
            return None

        if not 6 < len(event_code) < 20:
            return None

        # check event code is only printable characters
        if not all(c in string.printable for c in event_code):
            return None

        return await self.__is_event_authorized(event_code)
