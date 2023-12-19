""" Management routes """

from fastapi import APIRouter, FastAPI, Request

from ..authorize import Authorize

# pylint: disable=E0402
from ..management import (
    EventItemResponse,
    ManagementService,
    ModelDeploymentRequest,
    ModelDeploymentResponse,
    NewEventRequest,
    NewEventResponse,
)


class Management:
    """Management routes"""

    def __init__(
        self,
        app: FastAPI,
        authorize: Authorize,
        management: ManagementService,
        prefix: str,
        tags: list[str],
    ):
        self.app = app
        self.authorize = authorize
        self.management = management
        self.router = APIRouter()
        self.__include_router(prefix=prefix, tags=tags)

    def __include_router(self, prefix: str, tags: list[str]):
        """include router"""

        @self.router.get("/management/events/list/{query}", status_code=200)
        @self.router.get("/management/listevents/{query}", status_code=200, deprecated=True)
        async def management_list_active_events(query: str, request: Request) -> list[EventItemResponse]:
            """get event info"""

            # raises expection if not authenticated
            await self.authorize.authorize_management_access(request.headers)
            return await self.management.list_events(query)

        @self.router.post("/management/events/add", status_code=200)
        @self.router.post("/management/addevent", status_code=200, deprecated=True)
        async def management_add_new(event: NewEventRequest, request: Request) -> NewEventResponse:
            """get event info"""

            # raises expection if not authenticated
            await self.authorize.authorize_management_access(request.headers)
            return await self.management.add_new_event(event)

        @self.router.patch("/management/modeldeployment/upsert", status_code=200, response_model=None)
        async def management_deployment_upsert(deployment: ModelDeploymentRequest, request: Request) -> None:
            """get event info"""

            # raises expection if not authenticated
            await self.authorize.authorize_management_access(request.headers)
            return await self.management.upsert_model_deployment(deployment)

        # list model deployments
        @self.router.get("/management/modeldeployment/list/{query}", status_code=200)
        async def management_deployment_list(query: str, request: Request) -> list[ModelDeploymentResponse]:
            """get models deployed info"""

            # raises expection if not authenticated
            await self.authorize.authorize_management_access(request.headers)
            return await self.management.list_model_deployments(query)

        self.app.include_router(self.router, prefix=prefix, tags=tags)
