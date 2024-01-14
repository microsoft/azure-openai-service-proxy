""" usage logging """

import json
import logging
from uuid import UUID

import asyncpg
from fastapi import HTTPException
from pydantic import BaseModel

from .db_manager import DBManager

logging.basicConfig(level=logging.WARNING)


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
    api_key: UUID
    catalog_id: UUID | None = None

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
        api_key: UUID,
        catalog_id: UUID | None = None,
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
            api_key=api_key,
            catalog_id=catalog_id,
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

    def __init__(self, db_manager: DBManager):
        self.logging = logging.getLogger(__name__)
        self.db_manager = db_manager

    async def log_api_call(self, *, entity: MonitorEntity):
        """write event to Azure Storage account Queue called USAGE_LOGGING_NAME"""

        conn = await self.db_manager.get_connection()

        try:
            await conn.execute(
                "CALL aoai.add_attendee_metric($1, $2, $3)",
                entity.api_key,
                entity.event_id,
                entity.catalog_id,
            )

        except asyncpg.exceptions.PostgresError as error:
            self.logging.error("Postgres error: %s", str(error))
            raise HTTPException(
                status_code=503,
                detail="Error writing to monitor log.",
            ) from error

        except Exception as exception:
            logging.error("General exception in event_authorized: %s", str(exception))
            raise HTTPException(
                status_code=401,
                detail="Monitor update failed. General exception.",
            ) from exception
