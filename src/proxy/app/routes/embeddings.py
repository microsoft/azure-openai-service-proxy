""" OpenAI Embeddings API route """

from typing import Any

from app.config import Deployment
from app.routes.request_manager import RequestManager
from fastapi import Request, Response
from pydantic import BaseModel

# pylint: disable=E0402
from ..openai_async import OpenAIAsyncManager


class EmbeddingsRequest(BaseModel):
    """OpenAI Chat Request"""

    input: str | list[str]
    model: str = ""


class Embeddings(RequestManager):
    """Embeddings route"""

    def include_router(self):
        """include router"""

        # Support for Azure OpenAI Service SDK 1.0+
        @self.router.post(
            "/openai/deployments/{deployment_name}/embeddings",
            status_code=200,
            response_model=None,
        )
        async def oai_embeddings(
            model: EmbeddingsRequest,
            request: Request,
            response: Response,
            deployment_name: str = None,
        ) -> Any:
            """OpenAI chat completion response"""

            completion, status_code = await self.process_request(
                deployment_name=deployment_name,
                request=request,
                model=model,
                call_method=self.call_openai,
            )

            response.status_code = status_code
            return completion

        return self.router

    async def call_openai(
        self,
        model: object,
        deployment: Deployment,
        request: Request,
    ) -> Any:
        """call openai with retry"""

        url = (
            f"{deployment.endpoint_url}/openai/deployments/"
            f"{deployment.deployment_name}/embeddings"
            f"?api-version={self.api_version}"
        )

        openai_request = self.model_to_dict(model)
        async_mgr = OpenAIAsyncManager(deployment)
        response, http_status_code = await async_mgr.async_openai_post(openai_request, url)

        return response, http_status_code
