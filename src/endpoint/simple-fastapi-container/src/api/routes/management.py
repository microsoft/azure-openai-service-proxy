# routes/management.py

from fastapi import APIRouter, Request, Depends
from ..main import get_app

from ..management import (
    NewEventResponse,
    NewEventRequest,
    EventItemResponse,
    ModelDeploymentRequest,
    ModelDeploymentResponse,
)

router = APIRouter()


@router.get("/management/events/list/{query}", status_code=200)
@router.get("/management/listevents/{query}", status_code=200, deprecated=True)
async def management_list_active_events(
    query: str, request: Request, app=Depends(get_app)
) -> list[EventItemResponse]:
    """get event info"""

    # raises expection if not authenticated
    await app.state.authorize.authorize_management_access(request.headers)
    return await app.state.management.list_events(query)


@router.post("/management/events/add", status_code=200)
@router.post("/management/addevent", status_code=200, deprecated=True)
async def management_add_new(
    event: NewEventRequest, request: Request, app=Depends(get_app)
) -> NewEventResponse:
    """get event info"""

    # raises expection if not authenticated
    await app.state.authorize.authorize_management_access(request.headers)
    return await app.state.management.add_new_event(event)


@router.patch(
    "/management/modeldeployment/upsert", status_code=200, response_model=None
)
async def management_deployment_upsert(
    deployment: ModelDeploymentRequest, request: Request, app=Depends(get_app)
) -> None:
    """get event info"""

    # raises expection if not authenticated
    await app.state.authorize.authorize_management_access(request.headers)
    return await app.state.management.upsert_model_deployment(deployment)


# list model deployments
@router.get("/management/modeldeployment/list/{query}", status_code=200)
async def management_deployment_list(
    query: str, request: Request, app=Depends(get_app)
) -> list[ModelDeploymentResponse]:
    """get models deployed info"""

    # raises expection if not authenticated
    await app.state.authorize.authorize_management_access(request.headers)
    return await app.state.management.list_model_deployments(query)
