""" OpenAI Embeddings API route """

from fastapi import APIRouter, Request, Response, HTTPException, FastAPI
import openai.openai_object

# pylint: disable=E0402
from ..embeddings import EmbeddingsRequest


class Embeddings:
    """Embeddings route"""

    def __init__(self, app: FastAPI, prefix: str, tags: list[str]):
        self.app = app
        self.router = APIRouter()
        self.prefix = prefix
        self.tags = tags
        self.__include_router()

    def __include_router(self):
        """include router"""

        # Support for OpenAI SDK 0.28
        @self.router.post(
            "/engines/{engine_id}/embeddings",
            status_code=200,
            response_model=None,
        )
        # Support for Azure OpenAI Service SDK 1.0+
        @self.router.post(
            "/openai/deployments/{deployment_id}/embeddings",
            status_code=200,
            response_model=None,
        )
        # Support for OpenAI SDK 1.0+
        @self.router.post("/embeddings", status_code=200, response_model=None)
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
            _, user_token = await self.app.state.authorize.authorize_api_access(
                request.headers, deployment_id
            )

            if self.app.state.rate_limit_embeddings.is_call_rate_exceeded(user_token):
                raise HTTPException(
                    status_code=429,
                    detail="Rate limit exceeded. Try again in 10 seconds",
                )

            (
                completion,
                status_code,
            ) = await self.app.state.embeddings_mgr.call_openai_embeddings(embeddings)
            response.status_code = status_code
            return completion

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
