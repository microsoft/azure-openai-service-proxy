""" event_info route """

from fastapi import APIRouter, Request, FastAPI
from src.api.authorize import AuthorizeResponse


class EventInfo:
    """Completion route"""

    def __init__(self, app: FastAPI, prefix: str, tags: list[str]):
        self.app = app
        self.router = APIRouter()
        self.prefix = prefix
        self.tags = tags
        self.__include_router()

    def __include_router(self):
        """include router"""

        # This path is used by the playground
        @self.router.post("/eventinfo", status_code=200)
        async def event_info(
            request: Request,
        ) -> AuthorizeResponse:
            """get event info"""

            deployment_id = "event_info"

            # exception thrown if not authorized
            authorize_response, _ = await self.app.state.authorize.authorize_api_access(
                request.headers, deployment_id
            )

            return authorize_response

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
