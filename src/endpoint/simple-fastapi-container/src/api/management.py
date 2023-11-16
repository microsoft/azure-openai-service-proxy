""" Management API"""
import logging
import uuid
from datetime import datetime
from pydantic import BaseModel
from fastapi import HTTPException
from azure.core.exceptions import AzureError
from azure.data.tables import TableServiceClient, TableClient


logging.basicConfig(level=logging.WARNING)

MANAGEMENT_TABLE_NAME = "management"
EVENT_AUTHORIZATION_TABLE_NAME = "authorization"
EVENT_AUTHORISATION_PARTITION_KEY = "event"


class NewEventRequest(BaseModel):
    """New event object for Authorize class."""

    event_name: str
    start_utc: datetime
    end_utc: datetime
    max_token_cap: int
    event_url: str
    event_url_text: str
    contact_name: str
    contact_email: str
    organizer_name: str
    organizer_email: str

    def __init__(
        self,
        event_name: str,
        start_utc: datetime,
        end_utc: datetime,
        max_token_cap: int,
        event_url: str,
        event_url_text: str,
        contact_name: str,
        contact_email: str,
        organizer_name: str,
        organizer_email: str,
    ) -> None:
        super().__init__(
            event_name=event_name,
            start_utc=start_utc,
            end_utc=end_utc,
            max_token_cap=max_token_cap,
            event_url=event_url,
            event_url_text=event_url_text,
            contact_name=contact_name,
            contact_email=contact_email,
            organizer_name=organizer_name,
            organizer_email=organizer_email,
        )


# create a new EventResponse class
class NewEventResponse(BaseModel):
    """Response object for Authorize class."""

    event_code: str
    contact_name: str
    contact_email: str
    organizer_name: str
    organizer_email: str
    start_utc: datetime
    end_utc: datetime
    event_url: str
    event_name: str


class Management:
    """Management class for the API"""

    def __init__(self, connection_string: str):
        self.connection_string = connection_string
        self.logging = logging.getLogger(__name__)
        # check the management table exists if not, create it
        self._create_table()

    def _create_table(self):
        """Create the management table if it does not exist"""

        # Create management authorization table if it does not exist
        try:
            table_service_client = TableServiceClient.from_connection_string(
                conn_str=self.connection_string
            )

            table_service_client.create_table_if_not_exists(
                table_name=MANAGEMENT_TABLE_NAME
            )

            # check if there is a row in the table with a partition key of "management"
            table_client = TableClient.from_connection_string(
                conn_str=self.connection_string, table_name=MANAGEMENT_TABLE_NAME
            )

            with TableClient.from_connection_string(
                conn_str=self.connection_string, table_name=MANAGEMENT_TABLE_NAME
            ) as table_client:
                query_filter = "PartitionKey eq 'management'"
                # get all columns from the table
                queried_entities = table_client.query_entities(
                    query_filter=query_filter,
                    select=[
                        "*",
                    ],
                )

                # read the first row
                partition_key = None
                for entity in queried_entities:
                    partition_key = entity["PartitionKey"]

                if partition_key is None:
                    # create a new row with a partition key of "management" and a row key of of uuid
                    row_key = str(uuid.uuid4())

                    event_entity = {
                        "PartitionKey": "management",
                        "RowKey": row_key,
                        "Active": True,
                    }

                    # Add the event request to the table
                    table_client.create_entity(entity=event_entity)

        except AzureError as azure_error:
            self.logging.error(
                "HTTP response exception adding event request: %s", azure_error.message
            )
            raise HTTPException(
                status_code=504,
                detail="Create management authorization table failed. AzureError.",
            ) from azure_error

        except Exception as exception:
            self.logging.error(
                "General exception creating table: %s", exception.args[0]
            )
            raise HTTPException(
                status_code=500,
                detail="Create management authorization table failed.",
            ) from exception

    def add_new_event(self, event_request: NewEventRequest):
        """Add an event request to the management table"""

        row_key = str(uuid.uuid4())

        event_entity = {
            "PartitionKey": EVENT_AUTHORISATION_PARTITION_KEY,
            "RowKey": row_key,
            "Active": True,
            "EventName": event_request.event_name,
            "StartUTC": event_request.start_utc,
            "EndUTC": event_request.end_utc,
            "MaxTokenCap": event_request.max_token_cap,
            "EventUrl": event_request.event_url,
            "EventUrlText": event_request.event_url_text,
            "ContactName": event_request.contact_name,
            "ContactEmail": event_request.contact_email,
            "OrganizerName": event_request.organizer_name,
            "OrganizerEmail": event_request.organizer_email,
        }

        try:
            table_client = TableClient.from_connection_string(
                conn_str=self.connection_string,
                table_name=EVENT_AUTHORIZATION_TABLE_NAME,
            )

            # Add the event request to the table
            table_client.create_entity(entity=event_entity)

            # create and return a NewEventResponse
            return NewEventResponse(
                event_code=row_key,
                contact_name=event_request.contact_name,
                contact_email=event_request.contact_email,
                organizer_name=event_request.organizer_name,
                organizer_email=event_request.organizer_email,
                start_utc=event_request.start_utc,
                end_utc=event_request.end_utc,
                event_name=event_request.event_name,
                event_url=event_request.event_url,
            )

        except AzureError as azure_error:
            self.logging.error(
                "HTTP response exception adding event request: %s", azure_error.message
            )
            raise HTTPException(
                status_code=504,
                detail="Add event request failed. AzureError.",
            ) from azure_error

        except Exception as exception:
            self.logging.error(
                "General exception adding event request: %s", exception.args[0]
            )
            raise HTTPException(
                status_code=500,
                detail="Add event request failed.",
            ) from exception
