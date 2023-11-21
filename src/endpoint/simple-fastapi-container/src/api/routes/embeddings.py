from fastapi import APIRouter, Request, Response, Depends, HTTPException
from ..main import get_app
import openai.openai_object
from ..embeddings import EmbeddingsRequest


router = APIRouter()

# Support for OpenAI SDK 0.28
@router.post(
    "/engines/{engine_id}/embeddings",
    status_code=200,
    response_model=None,
)
# Support for Azure OpenAI Service SDK 1.0+
@router.post(
    "/openai/deployments/{deployment_id}/embeddings",
    status_code=200,
    response_model=None,
)
# Support for OpenAI SDK 1.0+
@router.post("/embeddings", status_code=200, response_model=None)
async def oai_embeddings(
    embeddings: EmbeddingsRequest,
    request: Request,
    response: Response,
    deployment_id: str = None,
    app=Depends(get_app),
) -> openai.openai_object.OpenAIObject:
    """OpenAI chat completion response"""

    # get the api version from the query string
    if "api-version" in request.query_params:
        embeddings.api_version = request.query_params["api-version"]

    # exception thrown if not authorized
    _, user_token = await app.state.authorize.authorize_api_access(
        request.headers, deployment_id
    )

    if app.state.rate_limit_embeddings.is_call_rate_exceeded(user_token):
        raise HTTPException(
            status_code=429,
            detail="Rate limit exceeded. Try again in 10 seconds",
        )

    (
        completion,
        status_code,
    ) = await app.state.embeddings_mgr.call_openai_embeddings(embeddings)
    response.status_code = status_code
    return completion
