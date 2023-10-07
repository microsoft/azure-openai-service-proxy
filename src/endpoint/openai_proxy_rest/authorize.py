""" Authorize a user to access playground based specific time bound event."""

import string
import logging
from datetime import datetime, timedelta
import pytz

from azure.data.tables.aio import TableClient

from azure.core.exceptions import (
    HttpResponseError,
    ServiceRequestError,
    ClientAuthenticationError,
)

CACHE_EXPIRY_MINUTES = 10


class Authorize:
    """Authorizes a user to access a specific time bound event."""

    def __init__(self, connection_string, table_name) -> None:
        self.connection_string = connection_string
        self.table_name = table_name
        self.event_cache = []
        self.cache_expiry = None

        logging.basicConfig(level=logging.WARNING)
        self.logger = logging.getLogger(__name__)

    async def event_authorized(self, event_code: str) -> bool:
        """checks if event code is in the cache, failing that get from table"""
        # """get event code, start date and end time from table"""
        # """add to cache dictionary object keyed by event code"""
        # """check the current utc time is between start and end time"""

        async with TableClient.from_connection_string(
            self.connection_string, self.table_name
        ) as table_client:
            # Is there a cache and is the event_code in the cache and not expired
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
                            current_time_utc = datetime.now(pytz.utc)
                            return start_utc <= current_time_utc <= end_utc

            try:
                name_filter = f"PartitionKey eq 'events' and RowKey eq '{event_code}'"
                queried_entities = table_client.query_entities(
                    query_filter=name_filter,
                    select=[
                        "start_utc",
                        "end_utc",
                    ],
                )

                async for entity in queried_entities:
                    start_utc = entity["start_utc"]
                    end_utc = entity["end_utc"]
                    current_time_utc = datetime.now(pytz.utc)

                    # set cache_expiry to current time plus 10 minutes
                    if not self.cache_expiry:
                        self.cache_expiry = datetime.now() + timedelta(
                            minutes=CACHE_EXPIRY_MINUTES
                        )

                    self.event_cache.append(
                        {event_code: {"start_utc": start_utc, "end_utc": end_utc}}
                    )

                    return start_utc <= current_time_utc <= end_utc

            except ClientAuthenticationError as auth_error:
                logging.error("ClientAuthenticationError: %s", auth_error.message)
                return False

            except ServiceRequestError as service_request_error:
                logging.error("ServiceResponseError: %s", service_request_error.message)
                return False

            except HttpResponseError as response_error:
                logging.error("HttpResponseError: %s", response_error.message)
                return False

            except Exception as exception:
                logging.error(
                    "General exception in event_authorized: %s", str(exception)
                )
                return False

        return False

    async def authorize(self, event_code: str) -> bool:
        """Authorizes a user to access a specific time bound event."""

        # check event_code is not empty
        if not event_code:
            return False

        if not 6 < len(event_code) < 20:
            return False

        # check event code is only printable characters
        if not all(c in string.printable for c in event_code):
            return False

        return await self.event_authorized(event_code)
