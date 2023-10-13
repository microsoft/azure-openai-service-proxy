""" FastAPI server for Azure OpenAI Service proxy """

import json
import logging
import os

from fastapi import FastAPI, HTTPException, Request, Response
from fastapi.exceptions import ResponseValidationError
from fastapi.responses import JSONResponse

from .authorize import Authorize, AuthorizeResponse
from .playground import Playground, PlaygroundRequest, PlaygroundResponse
from .configuration import OpenAIConfig


logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

OPENAI_API_VERSION = "2023-07-01-preview"

app = FastAPI()


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
async def openai_chat(request: Request) -> AuthorizeResponse:
    """get event info"""
    authorize_response = await __authorize(request.headers)

    if authorize_response is None or not authorize_response.is_authorized:
        raise HTTPException(
            status_code=401,
            detail="Event code is not authorized",
        )

    return authorize_response


@app.post("/api/oai_prompt", status_code=200)
async def event_info(
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
        raise HTTPException(
            status_code=500, detail=f"Failed to generate response: {exc}"
        ) from exc


@app.on_event("startup")
async def startup_event():
    """startup event"""

    try:
        deployment_string = os.environ["AZURE_OPENAI_DEPLOYMENTS"]
        deployments = json.loads(deployment_string)
        storage_connection_string = os.environ["AZURE_STORAGE_CONNECTION_STRING"]
        app.state.authorize = Authorize(storage_connection_string)
    except KeyError:
        print(
            "Please set the environment variables "
            "and AZURE_OPENAI_DEPLOYMENTS "
            "and AZURE_STORAGE_CONNECTION_STRING"
        )
        exit(1)

    openai_config = OpenAIConfig(
        openai_version=OPENAI_API_VERSION,
        config_connection_string=storage_connection_string,
        deployments=deployments,
    )

    app.state.openai_mgr = Playground(app, openai_config=openai_config)
    app.state.round_robin = 0


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=5500)
