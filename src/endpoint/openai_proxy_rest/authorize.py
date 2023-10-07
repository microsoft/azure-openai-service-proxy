""" Authorize a user to access playground based specific time bound event."""

import string
import pytz
import logging

from azure.data.tables.aio import TableClient

from azure.core.exceptions import (
    HttpResponseError,
    ServiceRequestError,
    ClientAuthenticationError,
)
from datetime import datetime


class Authorize:
    """Authorizes a user to access a specific time bound event."""

    def __init__(self, connection_string, table_name) -> None:
        self.connection_string = connection_string
        self.table_name = table_name

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
            try:
                name_filter = f"RowKey eq '{event_code}' and PartitionKey eq 'events'"
                queried_entities = table_client.query_entities(
                    query_filter=name_filter,
                    select=[
                        "start_utc",
                        "end_utc",
                    ],
                    # parameters=parameters,
                )
                async for entity in queried_entities:
                    start_time = entity["start_utc"]
                    end_time = entity["end_utc"]
                    current_time = datetime.now(pytz.utc)
                    return start_time <= current_time <= end_time

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
