""" routes for the attendee management """

import base64
import json
import logging

from fastapi import APIRouter, HTTPException, Request
from pydantic import BaseModel

logging.basicConfig(level=logging.INFO)


class AttendeeRegistrationResponse(BaseModel):
    """Attendee registration response"""


class AttendeeApi:
    """Attendee routes"""

    def __init__(self, db_manager):
        self.db_manager = db_manager
        self.router = APIRouter()
        self.logger = logging.getLogger(__name__)

    def include_router(self):
        """include router"""

        # This path is used by the attendee management pages
        @self.router.post("/attendee/event/{event_id}/register", status_code=201)
        async def register_attendee(
            request: Request, event_id: str
        ) -> AttendeeRegistrationResponse:
            """Register attendee"""
            logging.info("Registering attendee")
            try:
                header = request.headers.get("x-ms-client-principal")
                encoded = base64.b64decode(header)
                decoded = encoded.decode("ascii")
                principal = json.loads(decoded)

                user_id = principal.get("userId")

                conn = await self.db_manager.get_connection()

                result = await conn.fetch(
                    "SELECT * FROM aoai.add_event_attendee($1, $2)", user_id, event_id
                )

                if not result:
                    raise HTTPException(
                        status_code=404, detail=f"Event with id {event_id} not found"
                    )

                return AttendeeRegistrationResponse()
            except Exception as error:
                logging.error(error)
                raise error

        return self.router
