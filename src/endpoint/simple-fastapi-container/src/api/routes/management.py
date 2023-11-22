""" Management routes """

from fastapi import APIRouter, Request, FastAPI

from src.api.management import (
    NewEventResponse,
    NewEventRequest,
    EventItemResponse,
    ModelDeploymentRequest,
    ModelDeploymentResponse,
)


class Management:
    """Management routes"""

    def __init__(self, app: FastAPI, prefix: str, tags: list[str]):
        self.app = app
        self.router = APIRouter()
        self.prefix = prefix
        self.tags = tags
        self.__include_router()

    def __include_router(self):
        """include router"""

        @self.router.get("/management/events/list/{query}", status_code=200)
        @self.router.get(
            "/management/listevents/{query}", status_code=200, deprecated=True
        )
        async def management_list_active_events(
            query: str, request: Request
        ) -> list[EventItemResponse]:
            """get event info"""

            # raises expection if not authenticated
            await self.app.state.authorize.authorize_management_access(request.headers)
            return await self.app.state.management.list_events(query)

        @self.router.post("/management/events/add", status_code=200)
        @self.router.post("/management/addevent", status_code=200, deprecated=True)
        async def management_add_new(
            event: NewEventRequest, request: Request
        ) -> NewEventResponse:
            """get event info"""

            # raises expection if not authenticated
            await self.app.state.authorize.authorize_management_access(request.headers)
            return await self.app.state.management.add_new_event(event)

        @self.router.patch(
            "/management/modeldeployment/upsert", status_code=200, response_model=None
        )
        async def management_deployment_upsert(
            deployment: ModelDeploymentRequest, request: Request
        ) -> None:
            """get event info"""

            # raises expection if not authenticated
            await self.app.state.authorize.authorize_management_access(request.headers)
            return await self.app.state.management.upsert_model_deployment(deployment)

        # list model deployments
        @self.router.get("/management/modeldeployment/list/{query}", status_code=200)
        async def management_deployment_list(
            query: str, request: Request
        ) -> list[ModelDeploymentResponse]:
            """get models deployed info"""

            # raises expection if not authenticated
            await self.app.state.authorize.authorize_management_access(request.headers)
            return await self.app.state.management.list_model_deployments(query)

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
