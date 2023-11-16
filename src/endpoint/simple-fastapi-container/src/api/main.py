""" FastAPI server for Azure OpenAI Service proxy """

import logging
import os
import openai.openai_object

from fastapi import FastAPI, HTTPException, Request, Response
from fastapi.exceptions import ResponseValidationError
from fastapi.responses import JSONResponse
from starlette.middleware.cors import CORSMiddleware

from .authorize import Authorize, AuthorizeResponse
from .chat_playground import Playground, PlaygroundResponse
from .chat_completions import (
    ChatCompletionsRequest,
    ChatCompletions,
)
from .completions import Completions, CompletionsRequest
from .image_generation import ImagesGenerations, ImagesGenerationsRequst

# from .chat_completions import ChatCompletions
from .embeddings import EmbeddingsRequest, Embeddings
from .config import OpenAIConfig
from .rate_limit import RateLimit


logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

app = FastAPI(
    # docs_url=None,  # Disable docs (Swagger UI)
    # redoc_url=None,  # Disable redoc
)

# Enable CORS for all orgins
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.exception_handler(ResponseValidationError)
async def validation_exception_handler(request, exc):
    """validation exception handler"""

    print("Caught Validation Error:%s ", str(exc))

    return JSONResponse(
        content={"message": "Response validation error"}, status_code=400
    )


@app.post("/v1/api/eventinfo", status_code=200)
# Legacy, move to versioned endpoints where possible
@app.post("/api/eventinfo", status_code=200)
async def event_info(request: Request) -> AuthorizeResponse:
    """get event info"""
    authorize_response = await app.state.authorize.authorize_playground_access(
        request.headers
    )

    if authorize_response is None or not authorize_response.is_authorized:
        raise HTTPException(
            status_code=401,
            detail="Event code is not authorized",
        )

    return authorize_response


# Support for OpenAI SDK 0.28
@app.post(
    "/v1/engines/{engine_id}/embeddings",
    status_code=200,
    response_model=None,
)
# Support for Azure OpenAI Service SDK 1.0+
@app.post(
    "/v1/openai/deployments/{deployment_id}/embeddings",
    status_code=200,
    response_model=None,
)
# Support for OpenAI SDK 1.0+
@app.post("/v1/embeddings", status_code=200, response_model=None)
async def oai_embeddings(
    embeddings: EmbeddingsRequest,
    request: Request,
    response: Response,
    deployment_id: str = None,
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

    try:
        (
            completion,
            status_code,
        ) = await app.state.embeddings_mgr.call_openai_embeddings(embeddings)
        response.status_code = status_code
        return completion
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"{exc}") from exc


# Support for OpenAI SDK 0.28
@app.post(
    "/v1/engines/{engine_id}/completions",
    status_code=200,
    response_model=None,
)
# Support for Azure OpenAI Service SDK 1.0+
@app.post(
    "/v1/openai/deployments/{deployment_id}/completions",
    status_code=200,
    response_model=None,
)
# Support for OpenAI SDK 1.0+
@app.post("/v1/completions", status_code=200, response_model=None)
async def oai_completion(
    completion_request: CompletionsRequest,
    request: Request,
    response: Response,
    deployment_id: str = None,
) -> openai.openai_object.OpenAIObject | str:
    """OpenAI completion response"""

    # get the api version from the query string
    if "api-version" in request.query_params:
        completion_request.api_version = request.query_params["api-version"]

    # exception thrown if not authorized
    authorize_response, user_token = await app.state.authorize.authorize_api_access(
        request.headers, deployment_id
    )

    if app.state.rate_limit_chat_completion.is_call_rate_exceeded(user_token):
        raise HTTPException(
            status_code=429,
            detail="Rate limit exceeded. Try again in 10 seconds",
        )

    try:
        if (
            completion_request.max_tokens
            and completion_request.max_tokens > authorize_response.max_token_cap
        ):
            completion_request.max_tokens = authorize_response.max_token_cap

        (
            completion_response,
            status_code,
        ) = await app.state.completions_mgr.call_openai_completion(completion_request)
        response.status_code = status_code
        return completion_response
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"{exc}") from exc


# Support for OpenAI SDK 0.28
@app.post(
    "/v1/engines/{engine_id}/chat/completions",
    status_code=200,
    response_model=None,
)
# Support for Azure OpenAI Service SDK 1.0+
@app.post(
    "/v1/openai/deployments/{deployment_id}/chat/completions",
    status_code=200,
    response_model=None,
)
# Support for OpenAI SDK 1.0+
@app.post("/v1/chat/completions", status_code=200, response_model=None)
async def oai_chat_completion(
    chat: ChatCompletionsRequest,
    request: Request,
    response: Response,
    deployment_id: str = None,
) -> openai.openai_object.OpenAIObject | str:
    """OpenAI chat completion response"""

    # get the api version from the query string
    if "api-version" in request.query_params:
        chat.api_version = request.query_params["api-version"]

    # exception thrown if not authorized
    authorize_response, user_token = await app.state.authorize.authorize_api_access(
        request.headers, deployment_id
    )

    if app.state.rate_limit_chat_completion.is_call_rate_exceeded(user_token):
        raise HTTPException(
            status_code=429,
            detail="Rate limit exceeded. Try again in 10 seconds",
        )

    try:
        if chat.max_tokens and chat.max_tokens > authorize_response.max_token_cap:
            chat.max_tokens = authorize_response.max_token_cap

        (
            completion,
            status_code,
        ) = await app.state.chat_completions_mgr.call_openai_chat_completion(chat)
        response.status_code = status_code
        return completion
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"{exc}") from exc


# Support for OpenAI SDK 0.28
@app.post(
    "/v1/engines/{engine_id}/images/generations",
    status_code=200,
    response_model=None,
)
# Support for Azure OpenAI Service SDK 1.0+
@app.post(
    "/v1/openai/images/generations:submit",
    status_code=200,
    response_model=None,
)
# Support for OpenAI SDK 1.0+
@app.post("/v1/images/generations", status_code=200, response_model=None)
async def oai_images_generations(
    image_generation_request: ImagesGenerationsRequst,
    request: Request,
    response: Response,
):
    """OpenAI image generation response"""

    # No deployment_is passed for images generation so set to dall-e
    deployment_id = "dall-e"

    # get the api version from the query string
    if "api-version" in request.query_params:
        image_generation_request.api_version = request.query_params["api-version"]

    # exception thrown if not authorized
    _, user_token = await app.state.authorize.authorize_api_access(
        request.headers, deployment_id
    )

    if app.state.rate_limit_images_generations.is_call_rate_exceeded(user_token):
        raise HTTPException(
            status_code=429,
            detail="Rate limit exceeded. Try again in 10 seconds",
        )

    try:
        (
            completion_response,
            status_code,
        ) = await app.state.images_generations_mgr.call_openai_images_generations(
            image_generation_request
        )
        response.status_code = status_code
        return completion_response
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"{exc}") from exc


@app.post("/v1/api/playground", status_code=200)
@app.post("/api/oai_prompt", status_code=200)
async def oai_playground(
    chat: ChatCompletionsRequest, request: Request, response: Response
) -> PlaygroundResponse | str:
    """playground chat returns chat response"""

    # exception thrown if not authorized
    authorize_response = await app.state.authorize.authorize_playground_access(
        request.headers
    )

    try:
        if chat.max_tokens > authorize_response.max_token_cap:
            chat.max_tokens = authorize_response.max_token_cap

        completion, status_code = await app.state.openai_mgr.call_chat_playground(chat)
        response.status_code = status_code
        return completion
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"{exc}") from exc


@app.on_event("startup")
async def startup_event():
    """startup event"""

    try:
        storage_connection_string = os.environ["AZURE_STORAGE_CONNECTION_STRING"]
    except KeyError:
        print("Please set the environment variable AZURE_STORAGE_CONNECTION_STRING")
        exit(1)

    app.state.authorize = Authorize(storage_connection_string)
    openai_config_chat_completions = OpenAIConfig(
        connection_string=storage_connection_string,
        model_class="openai-chat",
    )

    openai_config_completions = OpenAIConfig(
        connection_string=storage_connection_string,
        model_class="openai-completions",
    )

    openai_config_embeddings = OpenAIConfig(
        connection_string=storage_connection_string,
        model_class="openai-embeddings",
    )

    openai_config_images_generations = OpenAIConfig(
        connection_string=storage_connection_string,
        model_class="openai-images-generations",
    )

    app.state.openai_mgr = Playground(openai_config=openai_config_chat_completions)

    app.state.chat_completions_mgr = ChatCompletions(
        openai_config=openai_config_chat_completions
    )

    app.state.completions_mgr = Completions(openai_config=openai_config_completions)

    app.state.embeddings_mgr = Embeddings(openai_config=openai_config_embeddings)

    app.state.images_generations_mgr = ImagesGenerations(
        openai_config=openai_config_images_generations
    )

    app.state.rate_limit_chat_completion = RateLimit()
    app.state.rate_limit_embeddings = RateLimit()
    app.state.rate_limit_images_generations = RateLimit()


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=5500)
