""" OpenAI Embeddings API route """

from typing import Any

from fastapi import FastAPI, Request, Response

from ..authorize import Authorize
from ..embeddings import Embeddings as RequestMgr
from ..embeddings import EmbeddingsRequest
from ..management import DeploymentClass

# pylint: disable=E0402
from .request_manager import RequestManager


class Embeddings(RequestManager):
    """Embeddings route"""

    def __init__(
        self,
        app: FastAPI,
        connection_string: str,
        prefix: str,
        tags: list[str],
        authorize: Authorize,
    ):
        super().__init__(
            app=app,
            authorize=authorize,
            connection_string=connection_string,
            prefix=prefix,
            tags=tags,
            deployment_class=DeploymentClass.OPENAI_EMBEDDINGS.value,
            request_class_mgr=RequestMgr,
        )

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
        ) -> Any:
            """OpenAI chat completion response"""

            # get the api version from the query string
            if "api-version" in request.query_params:
                embeddings.api_version = request.query_params["api-version"]

            # exception thrown if not authorized
            await self.authorize_request(deployment_id=deployment_id, request=request)

            (
                completion,
                status_code,
            ) = await self.request_class_mgr.call_openai_embeddings(embeddings)
            response.status_code = status_code
            return completion

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
