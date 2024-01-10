""" FastAPI server for Azure OpenAI Service proxy """

import logging
import os

from fastapi import FastAPI, HTTPException
from fastapi.exceptions import ResponseValidationError
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from .authorize import Authorize
from .config import Config

# pylint: disable=E0402
from .db_manager import DBManager
from .monitor import Monitor
from .routes.chat_completions import ChatCompletions as chat_completions_router
from .routes.completions import Completions as completions_router
from .routes.embeddings import Embeddings as embeddings_router
from .routes.event_info import EventInfo as eventinfo_router
from .routes.images import Images as images_router
from .routes.images_generations import ImagesGenerations as images_generations_router

DEFAULT_COMPLETIONS_API_VERSION = "2023-09-01-preview"
DEFAULT_CHAT_COMPLETIONS_API_VERSION = "2023-09-01-preview"
DEFAULT_EMBEDDINGS_API_VERSION = "2023-08-01-preview"
DEFAULT_CHAT_COMPLETIONS_EXTENSIONS_API_VERSION = "2023-08-01-preview"
DEFAULT_IMAGES_GENERATIONS_API_VERSION = "2023-06-01-preview"
OPENAI_IMAGES_API_VERSION = "2023-12-01-preview"

try:
    storage_connection_string = os.environ["AZURE_STORAGE_CONNECTION_STRING"]
except KeyError as key_error:
    print("Please set the environment variable AZURE_STORAGE_CONNECTION_STRING")
    raise HTTPException(
        status_code=500,
        detail="Please set the environment variable AZURE_STORAGE_CONNECTION_STRING",
    ) from key_error

try:
    sql_connection_string = os.environ["POSTGRES_CONNECTION_STRING"]
except KeyError as key_error:
    print("Please set the environment variable POSTGRES_CONNECTION_STRING")
    raise HTTPException(
        status_code=500,
        detail="Please set the environment variable POSTGRES_CONNECTION_STRING",
    ) from key_error


logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

app = FastAPI(
    # docs_url=None,  # Disable docs (Swagger UI)
    # redoc_url=None,  # Disable redoc
)

monitor = Monitor(connection_string=storage_connection_string)
db_manager = DBManager(connection_string=sql_connection_string)
authorize = Authorize(connection_string=storage_connection_string, db_manager=db_manager)
config = Config(db_manager=db_manager, monitor=monitor)


completion_router = completions_router(
    authorize=authorize,
    config=config,
    api_version=DEFAULT_COMPLETIONS_API_VERSION,
)

chat_route = chat_completions_router(
    authorize=authorize,
    config=config,
    api_version=DEFAULT_CHAT_COMPLETIONS_API_VERSION,
)


embeddings_route = embeddings_router(
    authorize=authorize,
    config=config,
    api_version=DEFAULT_EMBEDDINGS_API_VERSION,
)

event_info_route = eventinfo_router(
    authorize=authorize,
    config=config,
    api_version=None,
)

images_generations_route = images_generations_router(
    authorize=authorize,
    config=config,
    api_version=DEFAULT_IMAGES_GENERATIONS_API_VERSION,
)

images_route = images_router(
    authorize=authorize,
    config=config,
    api_version=OPENAI_IMAGES_API_VERSION,
)

app.include_router(completion_router.include_router(), prefix="/api/v1", tags=["completions"])
app.include_router(chat_route.include_router(), prefix="/api/v1", tags=["chat-completions"])
app.include_router(embeddings_route.include_router(), prefix="/api/v1", tags=["embeddings"])
app.include_router(event_info_route.include_router(), prefix="/api/v1", tags=["eventinfo"])
app.include_router(
    images_generations_route.include_router(), prefix="/api/v1", tags=["images-generations"]
)
app.include_router(images_route.include_router(), prefix="/api/v1", tags=["images"])


@app.exception_handler(ResponseValidationError)
async def validation_exception_handler(request, exc):
    """validation exception handler"""

    print("Caught Validation Error:%s ", str(exc))

    return JSONResponse(content={"message": "Response validation error"}, status_code=400)


@app.on_event("startup")
async def startup_event():
    """startup event"""


if os.environ.get("ENVIRONMENT") == "development":
    app.add_middleware(
        CORSMiddleware,
        allow_origins=["*"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=5500)
