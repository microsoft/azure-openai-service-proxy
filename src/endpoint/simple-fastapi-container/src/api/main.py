""" FastAPI server for Azure OpenAI Service proxy """

import logging
import os


from fastapi import FastAPI, HTTPException
from fastapi.exceptions import ResponseValidationError
from fastapi.responses import JSONResponse
from fastapi.staticfiles import StaticFiles
from fastapi.middleware.cors import CORSMiddleware

from .authorize import Authorize
from .chat_completions import (
    ChatCompletions,
)
from .completions import Completions
from .image_generation import ImagesGenerations
from .images import Images

from .embeddings import Embeddings
from .configuration import OpenAIConfig
from .rate_limit import RateLimit
from .management import (
    Management,
    DeploymentClass,
)


from .routes.management import Management as management_router
from .routes.embeddings import Embeddings as embeddings_router
from .routes.completions import Completions as completions_router
from .routes.chat_completions import ChatCompletions as chat_completions_router
from .routes.images import Images as images_router
from .routes.images_generations import ImagesGenerations as images_generations_router
from .routes.event_info import EventInfo as eventinfo_router


logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)

app = FastAPI(
    # docs_url=None,  # Disable docs (Swagger UI)
    # redoc_url=None,  # Disable redoc
)


management_router(app, "/v1/api", ["management"])
embeddings_router(app, "/v1/api", ["embeddings"])
completions_router(app, "/v1/api", ["completions"])
chat_completions_router(app, "/v1/api", ["chat_completions"])
images_router(app, "/v1/api", ["images"])
images_generations_router(app, "/v1/api", ["images_generations"])
eventinfo_router(app, "/v1/api", ["eventinfo"])


@app.exception_handler(ResponseValidationError)
async def validation_exception_handler(request, exc):
    """validation exception handler"""

    print("Caught Validation Error:%s ", str(exc))

    return JSONResponse(
        content={"message": "Response validation error"}, status_code=400
    )


@app.on_event("startup")
async def startup_event():
    """startup event"""

    try:
        storage_connection_string = os.environ["AZURE_STORAGE_CONNECTION_STRING"]
    except KeyError as key_error:
        print("Please set the environment variable AZURE_STORAGE_CONNECTION_STRING")
        raise HTTPException(
            status_code=500,
            detail="Please set the environment variable AZURE_STORAGE_CONNECTION_STRING",
        ) from key_error
        # exit(1)

    app.state.authorize = Authorize(storage_connection_string)
    app.state.management = Management(storage_connection_string)
    openai_config_chat_completions = OpenAIConfig(
        connection_string=storage_connection_string,
        model_class=DeploymentClass.OPENAI_CHAT.value,
    )

    openai_config_completions = OpenAIConfig(
        connection_string=storage_connection_string,
        model_class=DeploymentClass.OPENAI_COMPLETIONS.value,
    )

    openai_config_embeddings = OpenAIConfig(
        connection_string=storage_connection_string,
        model_class=DeploymentClass.OPENAI_EMBEDDINGS.value,
    )

    openai_config_images = OpenAIConfig(
        connection_string=storage_connection_string,
        model_class=DeploymentClass.OPENAI_IMAGES.value,
    )

    openai_config_images_generations = OpenAIConfig(
        connection_string=storage_connection_string,
        model_class=DeploymentClass.OPENAI_IMAGES_GENERATIONS.value,
    )

    app.state.chat_completions_mgr = ChatCompletions(
        openai_config=openai_config_chat_completions
    )

    app.state.completions_mgr = Completions(openai_config=openai_config_completions)

    app.state.embeddings_mgr = Embeddings(openai_config=openai_config_embeddings)

    app.state.images_generations_mgr = ImagesGenerations(
        openai_config=openai_config_images_generations
    )

    app.state.images_mgr = Images(openai_config=openai_config_images)

    app.state.rate_limit_chat_completion = RateLimit()
    app.state.rate_limit_embeddings = RateLimit()
    app.state.rate_limit_images_generations = RateLimit()


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
