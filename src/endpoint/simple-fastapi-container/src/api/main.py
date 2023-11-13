""" FastAPI server for Azure OpenAI Service proxy """

import logging
import os
import openai.openai_object

from fastapi import FastAPI, HTTPException, Request, Response
from fastapi.exceptions import ResponseValidationError
from fastapi.responses import JSONResponse

from .authorize import Authorize, AuthorizeResponse
from .chat_playground import Playground, PlaygroundResponse
from .chat_completions import (
    ChatCompletionsRequest,
    ChatCompletions,
)

# from .chat_completions import ChatCompletions
from .embeddings import EmbeddingsRequest, Embeddings
from .config import OpenAIConfig
from .rate_limit import RateLimit


logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

OPENAI_CHAT_COMPLETIONS_API_VERSION = "2023-07-01-preview"
OPENAI_EMBEDDINGS_API_VERSION = "2023-08-01-preview"

app = FastAPI(
    docs_url=None,  # Disable docs (Swagger UI)
    redoc_url=None,  # Disable redoc
)


@app.exception_handler(ResponseValidationError)
async def validation_exception_handler(request, exc):
    """validation exception handler"""

    print("Caught Validation Error:%s ", str(exc))

    return JSONResponse(
        content={"message": "Response validation error"}, status_code=400
    )


async def authorize(headers) -> AuthorizeResponse | None:
    """get the event code from the header"""
    if "openai-event-code" in headers:
        return await app.state.authorize.authorize(headers.get("openai-event-code"))
    return None


async def authorize_api_access(headers) -> (list | None, str | None):
    """validate chat complettion API request"""

    if "Authorization" in headers:
        auth_header = headers.get("Authorization")
        auth_parts = auth_header.split(" ")
        if len(auth_parts) != 2 or auth_parts[0] != "Bearer":
            return None, None
        id_parts = auth_parts[1].split("/")
        if len(id_parts) == 2 and len(id_parts[0]) > 0 and len(id_parts[1]) > 0:
            return await app.state.authorize.authorize(id_parts[0]), id_parts[1]

    return None, None


@app.post("/api/eventinfo", status_code=200)
async def event_info(request: Request) -> AuthorizeResponse:
    """get event info"""
    authorize_response = await authorize(request.headers)

    if authorize_response is None or not authorize_response.is_authorized:
        raise HTTPException(
            status_code=401,
            detail="Event code is not authorized",
        )

    return authorize_response


@app.post("/embeddings", status_code=200, response_model=None)
async def oai_embeddings(
    chat: EmbeddingsRequest, request: Request, response: Response
) -> openai.openai_object.OpenAIObject:
    """OpenAI chat completion response"""

    authorize_response, user_token = await authorize_api_access(request.headers)

    if authorize_response is None:
        raise HTTPException(
            status_code=401,
            detail="Invalid event_code/github_id",
        )

    # if app.state.rate_limit_embeddings.is_call_rate_exceeded(user_token):
    #     raise HTTPException(
    #         status_code=429,
    #         detail="Rate limit exceeded. Try again in 10 seconds",
    #     )

    try:
        (
            completion,
            status_code,
        ) = await app.state.embeddings.call_openai_embeddings(chat)
        response.status_code = status_code
        return completion
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"{exc}") from exc


@app.post("/chat/completions", status_code=200, response_model=None)
async def oai_chat_complettion(
    chat: ChatCompletionsRequest, request: Request, response: Response
) -> openai.openai_object.OpenAIObject | str:
    """OpenAI chat completion response"""

    authorize_response, user_token = await authorize_api_access(request.headers)

    if authorize_response is None:
        raise HTTPException(
            status_code=401,
            detail="Invalid event_code/github_id",
        )

    # if app.state.rate_limit_chat_completion.is_call_rate_exceeded(user_token):
    #     raise HTTPException(
    #         status_code=429,
    #         detail="Rate limit exceeded. Try again in 10 seconds",
    #     )

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


@app.post("/api/oai_prompt", status_code=200)
async def oai_playground(
    chat: ChatCompletionsRequest, request: Request, response: Response
) -> PlaygroundResponse | str:
    """playground chat returns chat response"""

    authorize_response = await authorize(request.headers)

    if authorize_response is None or not authorize_response.is_authorized:
        raise HTTPException(
            status_code=401,
            detail="Event code is not authorized",
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
    openai_config_completions = OpenAIConfig(
        api_version=OPENAI_CHAT_COMPLETIONS_API_VERSION,
        connection_string=storage_connection_string,
        model_class="openai-chat",
    )

    openai_config_embeddings = OpenAIConfig(
        api_version=OPENAI_EMBEDDINGS_API_VERSION,
        connection_string=storage_connection_string,
        model_class="openai-embeddings",
    )

    app.state.openai_mgr = Playground(app, openai_config=openai_config_completions)
    app.state.chat_completions_mgr = ChatCompletions(
        app, openai_config=openai_config_completions
    )
    app.state.embeddings = Embeddings(app, openai_config=openai_config_embeddings)
    app.state.rate_limit_chat_completion = RateLimit()
    app.state.rate_limit_embeddings = RateLimit()


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=5500)
