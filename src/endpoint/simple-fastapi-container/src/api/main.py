""" FastAPI server for Azure OpenAI Service proxy """

import logging
import os

import openai.openai_object

from fastapi import FastAPI, HTTPException, Request, Response
from fastapi.exceptions import ResponseValidationError
from fastapi.responses import JSONResponse

from .authorize import Authorize, AuthorizeResponse
from .playground import Playground, PlaygroundRequest, PlaygroundResponse
from .chat_completion import ChatCompletion
from .configuration import OpenAIConfig


logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

OPENAI_API_VERSION = "2023-07-01-preview"

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


async def __authorize(headers) -> AuthorizeResponse | None:
    """get the event code from the header"""
    if "openai-event-code" in headers:
        return await app.state.authorize.authorize(headers.get("openai-event-code"))
    return None


@app.post("/api/eventinfo", status_code=200)
async def event_info(request: Request) -> AuthorizeResponse:
    """get event info"""
    authorize_response = await __authorize(request.headers)

    if authorize_response is None or not authorize_response.is_authorized:
        raise HTTPException(
            status_code=401,
            detail="Event code is not authorized",
        )

    return authorize_response


@app.post("/v1/chat/completions", status_code=200, response_model=None)
async def oai_chat_complettion(
    chat: PlaygroundRequest, request: Request, response: Response
) -> openai.openai_object.OpenAIObject:
    """OpenAI chat completion response"""

    authorize_response = await __authorize(request.headers)

    if authorize_response is None or not authorize_response.is_authorized:
        raise HTTPException(
            status_code=401,
            detail="Event code is not authorized",
        )

    try:
        if chat.max_tokens > authorize_response.max_token_cap:
            chat.max_tokens = authorize_response.max_token_cap

        (
            completion,
            status_code,
        ) = await app.state.chat_completion_mgr.call_openai_chat_completion(chat)
        response.status_code = status_code
        return completion
    except Exception as exc:
        raise HTTPException(status_code=500, detail=f"{exc}") from exc


@app.post("/api/oai_prompt", status_code=200)
async def oai_playground(
    chat: PlaygroundRequest, request: Request, response: Response
) -> PlaygroundResponse:
    """playground chat returns chat response"""

    authorize_response = await __authorize(request.headers)

    if authorize_response is None or not authorize_response.is_authorized:
        raise HTTPException(
            status_code=401,
            detail="Event code is not authorized",
        )

    try:
        if chat.max_tokens > authorize_response.max_token_cap:
            chat.max_tokens = authorize_response.max_token_cap

        completion, status_code = await app.state.openai_mgr.call_openai_chat(chat)
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
    openai_config = OpenAIConfig(
        openai_version=OPENAI_API_VERSION,
        connection_string=storage_connection_string,
    )

    app.state.openai_mgr = Playground(app, openai_config=openai_config)
    app.state.chat_completion_mgr = ChatCompletion(app, openai_config=openai_config)


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=5500)
