""" usage logging """

import base64
import json
import logging
from uuid import UUID

from azure.core.exceptions import AzureError
from azure.storage.queue import QueueServiceClient
from fastapi import HTTPException
from pydantic import BaseModel

logging.basicConfig(level=logging.WARNING)

USAGE_LOGGING_NAME = "monitor"


class Usage:
    """Usage"""

    def __init__(self):
        self.count = 0

    def reset(self):
        """reset"""

        self.count = 0

    def count_tokens(self, chunk: bytes):
        """increment"""

        chunk = chunk.decode("ascii")
        data_segments = chunk.split("data: ")

        for data in data_segments:
            if data:
                if "[DONE]\n\n" == data:
                    self.count -= 1
                    break

                if '"finish_reason":null' in data:
                    self.count += 1

        print(f"Usage count: {self.count}")


class MonitorEntity(BaseModel):
    """Response object for Authorize class."""

    is_authorized: bool
    max_token_cap: int
    daily_request_cap: int
    user_id: str
    event_id: str
    event_code: str
    event_url: str
    event_url_text: str
    organizer_name: str
    organizer_email: str
    deployment_name: str

    def __init__(
        self,
        is_authorized: bool,
        max_token_cap: int,
        daily_request_cap: int,
        user_id: str,
        event_id: str,
        event_code: str,
        event_url: str,
        event_url_text: str,
        organizer_name: str,
        organizer_email: str,
        deployment_name: str,
    ) -> None:
        super().__init__(
            is_authorized=is_authorized,
            max_token_cap=max_token_cap,
            daily_request_cap=daily_request_cap,
            user_id=user_id,
            event_id=event_id,
            event_code=event_code,
            event_url=event_url,
            event_url_text=event_url_text,
            organizer_name=organizer_name,
            organizer_email=organizer_email,
            deployment_name=deployment_name,
        )


class UUIDEncoder(json.JSONEncoder):
    """UUID Encoder"""

    def default(self, o):
        """default"""
        if isinstance(o, UUID):
            # if the obj is uuid, we simply return the value of uuid
            return o.hex
        return json.JSONEncoder.default(self, o)


class Monitor:
    """tracking"""

    def __init__(self, connection_string: str):
        self.logging = logging.getLogger(__name__)
        self.queue_service_client = QueueServiceClient.from_connection_string(connection_string)
        self.queue_client = self.queue_service_client.get_queue_client(USAGE_LOGGING_NAME)

    def log_api_call(self, *, entity: MonitorEntity):
        """write event to Azure Storage account Queue called USAGE_LOGGING_NAME"""

        message = json.dumps(entity.__dict__, cls=UUIDEncoder)
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
