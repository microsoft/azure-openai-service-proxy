""" FastAPI server for Azure OpenAI Service proxy """

import logging
import os

import pyodbc
from fastapi import FastAPI, HTTPException
from fastapi.exceptions import ResponseValidationError
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from fastapi.staticfiles import StaticFiles

# pylint: disable=E0402
from .authorize import Authorize
from .config import Config
from .routes.chat_completions import ChatCompletions as chat_completions_router
from .routes.chat_extensions import ChatExtensions as chat_extensions_router
from .routes.completions import Completions as completions_router
from .routes.embeddings import Embeddings as embeddings_router
from .routes.event_info import EventInfo as eventinfo_router
from .routes.images import Images as images_router
from .routes.images_generations import ImagesGenerations as images_generations_router

try:
    storage_connection_string = os.environ["AZURE_STORAGE_CONNECTION_STRING"]
except KeyError as key_error:
    print("Please set the environment variable AZURE_STORAGE_CONNECTION_STRING")
    raise HTTPException(
        status_code=500,
        detail="Please set the environment variable AZURE_STORAGE_CONNECTION_STRING",
    ) from key_error

try:
    sql_connection_string = os.environ["AZURE_SQL_CONNECTION_STRING"]
except KeyError as key_error:
    print("Please set the environment variable AZURE_SQL_CONNECTION_STRING")
    raise HTTPException(
        status_code=500,
        detail="Please set the environment variable AZURE_SQL_CONNECTION_STRING",
    ) from key_error


logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

app = FastAPI(
    # docs_url=None,  # Disable docs (Swagger UI)
    # redoc_url=None,  # Disable redoc
)


sql_connection_string = f"DRIVER={{ODBC Driver 18 for SQL Server}};{sql_connection_string}"

sql_conn = pyodbc.connect(sql_connection_string)
sql_conn.timeout = 10


authorize = Authorize(connection_string=storage_connection_string, sql_conn=sql_conn)
config = Config(sql_conn=sql_conn)


completion_router = completions_router(authorize=authorize, config=config)
app.include_router(completion_router.include_router(), prefix="/v1/api", tags=["completions"])

chat_route = chat_completions_router(authorize=authorize, config=config)
app.include_router(chat_route.include_router(), prefix="/v1/api", tags=["chat-completions"])

chat_extensions_route = chat_extensions_router(authorize=authorize, config=config)
app.include_router(
    chat_extensions_route.include_router(),
    prefix="/v1/api",
    tags=["chat-completions-extensions"],
)

embeddings_route = embeddings_router(authorize=authorize, config=config)
app.include_router(embeddings_route.include_router(), prefix="/v1/api", tags=["embeddings"])

event_info_route = eventinfo_router(authorize=authorize, config=config)
app.include_router(event_info_route.include_router(), prefix="/v1/api", tags=["eventinfo"])


images_generations_route = images_generations_router(authorize=authorize, config=config)
app.include_router(
    images_generations_route.include_router(),
    prefix="/v1/api",
    tags=["images-generations"],
)

images_route = images_router(authorize=authorize, config=config)
app.include_router(images_route.include_router(), prefix="/v1/api", tags=["images"])


@app.exception_handler(ResponseValidationError)
async def validation_exception_handler(request, exc):
    """validation exception handler"""

    print("Caught Validation Error:%s ", str(exc))

    return JSONResponse(content={"message": "Response validation error"}, status_code=400)


@app.on_event("startup")
async def startup_event():
    """startup event"""
    pass


STATIC_FILES_DIR = "playground/dist"

if os.environ.get("ENVIRONMENT") == "development":
    app.add_middleware(
        CORSMiddleware,
        allow_origins=["*"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    STATIC_FILES_DIR = "src/playground"

app.mount("/", StaticFiles(directory=STATIC_FILES_DIR, html=True), name="static")

if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=5500)
