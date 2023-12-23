""" event_info route """

from fastapi import Request
from pydantic import BaseModel

# pylint: disable=E0402
from .request_manager import RequestManager


class EventInfoResponse(BaseModel):
    """Event Info Response"""

    is_authorized: bool
    max_token_cap: int
    event_code: str
    event_url: str
    event_url_text: str
    organizer_name: str
    organizer_email: str
    capabilities: dict

    def __init__(
        self,
        is_authorized: bool,
        max_token_cap: int,
        event_code: str,
        event_url: str,
        event_url_text: str,
        organizer_name: str,
        organizer_email: str,
        capabilities: dict,
    ) -> None:
        super().__init__(
            is_authorized=is_authorized,
            max_token_cap=max_token_cap,
            event_code=event_code,
            event_url=event_url,
            event_url_text=event_url_text,
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

            event_info_dict = dict(authorize_response)
            event_info_dict["capabilities"] = capabilities
            del event_info_dict["user_id"]
            del event_info_dict["event_id"]
            del event_info_dict["deployment_name"]
            del event_info_dict["daily_request_cap"]

            return EventInfoResponse(**event_info_dict)

        return self.router
