""" event_info route """

from uuid import UUID

from fastapi import Request
from pydantic import BaseModel

# pylint: disable=E0402
from ..authorize import Authorize
from ..config import Config
from ..deployment_class import DeploymentClass
from .request_manager import RequestManager


class EventInfoResponse(BaseModel):
    """Event Info Response"""

    is_authorized: bool
    max_token_cap: int
    event_code: str
    user_token: UUID
    event_name: str
    event_url: str
    event_url_text: str
    organizer_name: str
    organizer_email: str
    capabilities: list[str]

    def __init__(
        self,
        is_authorized: bool,
        max_token_cap: int,
        event_code: str,
        user_token: UUID,
        event_name: str,
        event_url: str,
        event_url_text: str,
        organizer_name: str,
        organizer_email: str,
        capabilities: list[str],
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
            capabilities=capabilities,
        )


class EventInfo(RequestManager):
    """Completion route"""

    def __init__(self, authorize: Authorize, config: Config):
        super().__init__(
            authorize=authorize,
            config=config,
            deployment_class=DeploymentClass.EVENT_INFO.value,
        )

    def include_router(self):
        """include router"""

        # This path is used by the playground
        @self.router.post("/eventinfo", status_code=200)
        async def event_info(
            request: Request,
        ) -> EventInfoResponse:
            """get event info"""

            deployment_id = "event_info"

            # exception thrown if not authorized
            authorize_response = await self.authorize_request(
                request=request,
                deployment_id=deployment_id,
            )

            capabilities = await self.config.get_owner_models(authorize_response=authorize_response)

            # fill in EventInfoResponse from authorize_response
            event_info_response = EventInfoResponse(
                is_authorized=authorize_response.is_authorized,
                max_token_cap=authorize_response.max_token_cap,
                event_code=authorize_response.event_id,
                user_token=authorize_response.user_token,
                event_name=authorize_response.event_name,
                event_url=authorize_response.event_url,
                event_url_text=authorize_response.event_url_text,
                organizer_name=authorize_response.organizer_name,
                organizer_email=authorize_response.organizer_email,
                capabilities=capabilities,
            )

            return event_info_response

        return self.router
