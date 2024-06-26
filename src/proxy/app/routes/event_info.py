""" event_info route """

from app.routes.request_manager import RequestManager
from fastapi import Request
from pydantic import BaseModel


class EventInfoResponse(BaseModel):
    """Event Info Response"""

    is_authorized: bool
    max_token_cap: int
    event_code: str
    event_image_url: str | None = None
    organizer_name: str
    organizer_email: str
    capabilities: dict

    def __init__(
        self,
        is_authorized: bool,
        max_token_cap: int,
        event_code: str,
        event_image_url: str,
        organizer_name: str,
        organizer_email: str,
        capabilities: dict,
    ) -> None:
        super().__init__(
            is_authorized=is_authorized,
            max_token_cap=max_token_cap,
            event_code=event_code,
            event_image_url=event_image_url,
            organizer_name=organizer_name,
            organizer_email=organizer_email,
            capabilities=capabilities,
        )


class EventInfo(RequestManager):
    """Completion route"""

    def include_router(self):
        """include router"""

        # This path is used by the playground
        @self.router.post("/eventinfo", status_code=200)
        async def event_info(
            request: Request,
        ) -> EventInfoResponse:
            """get event info"""

            deployment_name = "event_info"

            # exception thrown if not authorized
            authorize_response = await self.authorize_request(
                request=request,
                deployment_name=deployment_name,
            )

            capabilities = await self.config.get_event_deployments(
                authorize_response=authorize_response
            )

            # create a new EventInfoResponse object from authorize_response
            event_info_response = EventInfoResponse(
                is_authorized=authorize_response.is_authorized,
                max_token_cap=authorize_response.max_token_cap,
                event_code=authorize_response.event_code,
                event_image_url=authorize_response.event_image_url,
                organizer_name=authorize_response.organizer_name,
                organizer_email=authorize_response.organizer_email,
                capabilities=capabilities,
            )

            return event_info_response

        return self.router
