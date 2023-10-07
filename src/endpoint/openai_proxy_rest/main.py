""" FastAPI server for Azure OpenAI Service proxy """

import json
import logging
import os

from fastapi import FastAPI, HTTPException, Request, Response
from fastapi.exceptions import ResponseValidationError
from fastapi.responses import JSONResponse

from authorize import Authorize
from openai_async import (
    OpenAIChatRequest,
    OpenAIChatResponse,
    OpenAIConfig,
)
from openai_async import (
    OpenAIManager as oai,
)

logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

GPT_MODEL_NAME = "gpt-3.5-turbo-0613"
OPENAI_API_VERSION = "2023-07-01-preview"

app = FastAPI()


@app.exception_handler(ResponseValidationError)
async def validation_exception_handler(request, exc):
    """validation exception handler"""

    print("Caught Validation Error:%s ", str(exc))

    return JSONResponse(
        content={"message": "Response validation error"}, status_code=400
    )


@app.post("/api/oai_prompt", status_code=200)
async def openai_chat(
    chat: OpenAIChatRequest, request: Request, response: Response
) -> OpenAIChatResponse:
    """openai chat returns chat response"""

    async def authorize(headers) -> bool:
        """get the event code from the header"""
        if "openai-event-code" in headers:
            return await app.state.authorize.authorize(headers.get("openai-event-code"))
        return False

    if not await authorize(request.headers):
        raise HTTPException(status_code=401, detail="Not authorized")

    try:
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
        storage_table_name = os.environ["AZURE_STORAGE_TABLE_NAME"]
        app.state.authorize = Authorize(storage_connection_string, storage_table_name)
    except KeyError:
        print(
            "Please set the environment variables "
            "and AZURE_OPENAI_DEPLOYMENTS "
            "and AZURE_STORAGE_CONNECTION_STRING"
        )
        exit(1)

    openai_config = OpenAIConfig(
        openai_version=OPENAI_API_VERSION,
        gpt_model_name=GPT_MODEL_NAME,
        deployments=deployments,
        request_timeout=30,
    )

    app.state.openai_mgr = oai(app, openai_config=openai_config)
    app.state.round_robin = 0


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=5500)
