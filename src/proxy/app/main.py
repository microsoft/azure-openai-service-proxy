""" FastAPI server for Azure OpenAI Service proxy """

import logging
import os

from fastapi import FastAPI, HTTPException, Request
from fastapi.exceptions import ResponseValidationError
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from .authorize import Authorize
from .config import Config

# pylint: disable=E0402
from .db_manager import DBConfig, DBManager
from .monitor import Monitor
from .routes.attendee import AttendeeApi as attendee_router
from .routes.azure_ai_search import AzureAISearch as azure_ai_search_router
from .routes.chat_completions import ChatCompletions as chat_completions_router
from .routes.completions import Completions as completions_router
from .routes.embeddings import Embeddings as embeddings_router
from .routes.event_info import EventInfo as eventinfo_router
from .routes.event_registration import EventRegistrationInfo as event_registration_router
from .routes.images import Images as images_router

DEFAULT_COMPLETIONS_API_VERSION = "2023-09-01-preview"
DEFAULT_CHAT_COMPLETIONS_API_VERSION = "2023-09-01-preview"
DEFAULT_EMBEDDINGS_API_VERSION = "2023-08-01-preview"
DEFAULT_CHAT_COMPLETIONS_EXTENSIONS_API_VERSION = "2023-08-01-preview"
DEFAULT_IMAGES_GENERATIONS_API_VERSION = "2023-06-01-preview"
DEFAULT_SEARCH_API_VERSION = "2023-11-01"
OPENAI_IMAGES_API_VERSION = "2023-12-01-preview"


logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

app = FastAPI(
    # docs_url=None,  # Disable docs (Swagger UI)
    # redoc_url=None,  # Disable redoc
)

db_config = DBConfig(
    host=os.environ.get("POSTGRES_HOST"),
    port=os.environ.get("POSTGRES_PORT", 5432),
    database=os.environ.get("POSTGRES_DATABASE", "aoai-proxy"),
    user=os.environ.get("POSTGRES_USER"),
    password=os.environ.get("POSTGRES_PASSWORD"),
    postgres_encryption_key=os.environ.get("POSTGRES_ENCRYPTION_KEY"),
    connection_string=os.environ.get("POSTGRES_CONNECTION_STRING"),
)


db_manager = DBManager(db_config=db_config)
monitor = Monitor(db_manager=db_manager)
authorize = Authorize(db_manager=db_manager)
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

images_route = images_router(
    authorize=authorize,
    config=config,
    api_version=OPENAI_IMAGES_API_VERSION,
)

event_registration_route = event_registration_router(db_manager=db_manager)

attendee_route = attendee_router(db_manager=db_manager)

search_route = azure_ai_search_router(
    authorize=authorize,
    config=config,
    api_version=DEFAULT_SEARCH_API_VERSION,
)

app.include_router(completion_router.include_router(), prefix="/api/v1", tags=["completions"])
app.include_router(chat_route.include_router(), prefix="/api/v1", tags=["chat-completions"])
app.include_router(embeddings_route.include_router(), prefix="/api/v1", tags=["embeddings"])
app.include_router(event_info_route.include_router(), prefix="/api/v1", tags=["eventinfo"])
app.include_router(images_route.include_router(), prefix="/api/v1", tags=["images"])
app.include_router(
    event_registration_route.include_router(), prefix="/api/v1", tags=["event-registration"]
)
app.include_router(attendee_route.include_router(), prefix="/api/v1", tags=["attendee"])
app.include_router(search_route.include_router(), prefix="/api/v1", tags=["search"])


@app.exception_handler(HTTPException)
async def custom_http_exception_handler(request: Request, exc: HTTPException):
    """custom http exception handler - formats in the style of OpenAI API"""

    content = {"error": {"code": exc.status_code, "message": exc.detail}}
    logger.error(msg=f"HTTPException: {content}")
    return JSONResponse(status_code=exc.status_code, content=content)


@app.exception_handler(ResponseValidationError)
async def validation_exception_handler(request, exc):
    """validation exception handler"""

    print("Caught Validation Error:%s ", str(exc))

    return JSONResponse(content={"message": "Response validation error"}, status_code=400)


@app.on_event("startup")
async def startup_event():
    """startup event"""
    await db_manager.create_pool()


@app.on_event("shutdown")
async def shutdown_event():
    """shutdown event"""
    await db_manager.close_pool()


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
