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

    def get_user_id(self, request: Request):
        """Get user id from request"""

        header = request.headers.get("x-ms-client-principal")

        if not header:
            raise HTTPException(status_code=401)

        encoded = base64.b64decode(header)
        decoded = encoded.decode("ascii")
        principal = json.loads(decoded)

        user_id = principal.get("userId")
        return user_id

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
                user_id = self.get_user_id(request=request)

                pool = await self.db_manager.get_connection()

                async with pool.acquire() as conn:
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

        @self.router.get("/attendee/event/{event_id}", status_code=200)
        async def get_attendees(request: Request, event_id: str):
            """Get attendees"""
            logging.info("Getting attendees")

            pool = await self.db_manager.get_connection()

            try:
                user_id = self.get_user_id(request=request)

                async with pool.acquire() as conn:
                    result = await conn.fetch(
                        "SELECT EA.api_key, EA.active "
                        "FROM aoai.event_attendee EA "
                        "WHERE EA.event_id = $1 "
                        "AND EA.user_id = $2",
                        event_id,
                        user_id,
                    )

                if not result:
                    raise HTTPException(
                        status_code=404, detail=f"Event with id {event_id} not found"
                    )

                if len(result) == 0:
                    raise HTTPException(
                        status_code=404, detail=f"Event with id {event_id} not found"
                    )

                return result[0]

            except Exception as error:
                logging.error(error)
                raise error

        @self.router.delete("/attendee/event/{event_id}/register", status_code=204)
        async def unregister_attendee(request: Request, event_id: str):
            """Unregister attendee"""
            logging.info("Unregistering attendee")

            try:
                user_id = self.get_user_id(request=request)

                pool = await self.db_manager.get_connection()

                async with pool.acquire() as conn:
                    await conn.fetch(
                        "UPDATE aoai.event_attendee "
                        "SET active = false "
                        "WHERE user_id = $1 AND event_id = $2",
                        user_id,
                        event_id,
                    )

            except Exception as error:
                logging.error(error)
                raise error

        @self.router.patch("/attendee/event/{event_id}/register", status_code=204)
        async def activate_user(request: Request, event_id: str):
            """Unregister attendee"""
            logging.info("Activate attendee")

            try:
                user_id = self.get_user_id(request=request)

                pool = await self.db_manager.get_connection()

                async with pool.acquire() as conn:
                    await conn.fetch(
                        "UPDATE aoai.event_attendee "
                        "SET active = true "
                        "WHERE user_id = $1 AND event_id = $2",
                        user_id,
                        event_id,
                    )

            except Exception as error:
                logging.error(error)
                raise error

        @self.router.delete("/attendee", status_code=204)
        async def delete_account(request: Request):
            """Delete account and all registrations"""
            logging.info("Delete account and all registrations")

            try:
                user_id = self.get_user_id(request=request)

                pool = await self.db_manager.get_connection()

                async with pool.acquire() as conn:
                    await conn.fetch(
                        "delete from aoai.event_attendee where user_id = $1",
                        user_id,
                    )

            except Exception as error:
                logging.error(error)
                raise error

        return self.router
