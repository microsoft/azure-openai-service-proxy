""" usage logging """

import base64
import logging
import json
from pydantic import BaseModel
from fastapi import HTTPException
from azure.core.exceptions import (
    AzureError,
)
from azure.storage.queue import QueueServiceClient

logging.basicConfig(level=logging.WARNING)

USAGE_LOGGING_NAME = "monitor"


class MonitorEntity(BaseModel):
    """Response object for Authorize class."""

    is_authorized: bool
    max_token_cap: int
    event_code: str
    user_token: str
    event_name: str
    event_url: str
    event_url_text: str
    organizer_name: str
    organizer_email: str
    request_class: str

    def __init__(
        self,
        is_authorized: bool,
        max_token_cap: int,
        event_code: str,
        user_token: str,
        event_name: str,
        event_url: str,
        event_url_text: str,
        organizer_name: str,
        organizer_email: str,
        request_class: str,
    ) -> None:
        super().__init__(
            is_authorized=is_authorized,
            max_token_cap=max_token_cap,
            event_code=event_code,
            user_token=user_token,
            event_name=event_name,
            event_url=event_url,
            event_url_text=event_url_text,
            organizer_name=organizer_name,
            organizer_email=organizer_email,
            request_class=request_class,
        )


class Monitor:
    """tracking"""

    def __init__(self, connection_string: str):
        self.logging = logging.getLogger(__name__)
        self.queue_service_client = QueueServiceClient.from_connection_string(
            connection_string
        )
        self.queue_client = self.queue_service_client.get_queue_client(
            USAGE_LOGGING_NAME
        )

    def log_api_call(self, *, entity: MonitorEntity):
        """write event to Azure Storage account Queue called USAGE_LOGGING_NAME"""

        # add the class name to the entity

        message = json.dumps(entity.__dict__)
        # base64 encode the message to a string
        message = base64.b64encode(message.encode("ascii")).decode("ascii")
        try:
            self.queue_client.send_message(message)
        except AzureError as azure_error:
            logging.error(" AzureError: %s", str(azure_error))

        except Exception as exception:
            logging.error("General exception in event_authorized: %s", str(exception))
            raise HTTPException(
                status_code=401,
                detail="Authentication failed. General exception.",
            ) from exception
