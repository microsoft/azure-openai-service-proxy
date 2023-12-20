""" OpenAI Embeddings API route """

from typing import Any

import openai.openai_object
from fastapi import Request, Response
from pydantic import BaseModel

# pylint: disable=E0402
from ..config import Deployment
from ..openai_async import OpenAIAsyncManager
from .request_manager import RequestManager

OPENAI_EMBEDDINGS_API_VERSION = "2023-08-01-preview"


class EmbeddingsRequest(BaseModel):
    """OpenAI Chat Request"""

    input: str | list[str]
    model: str = ""
    api_version: str = OPENAI_EMBEDDINGS_API_VERSION


class Embeddings(RequestManager):
    """Embeddings route"""

    def include_router(self):
        """include router"""

        # Support for Azure OpenAI Service SDK 1.0+
        @self.router.post(
            "/openai/deployments/{deployment_id}/embeddings",
            status_code=200,
            response_model=None,
        )
        async def oai_embeddings(
            model: EmbeddingsRequest,
            request: Request,
            response: Response,
            deployment_id: str = None,
        ) -> openai.openai_object.OpenAIObject:
            """OpenAI chat completion response"""

            completion, status_code = await self.process_request(
                deployment_id=deployment_id,
                request=request,
                model=model,
                call_method=self.call_openai,
            )

            response.status_code = status_code
            return completion

        return self.router

    async def call_openai(
        self,
        model: EmbeddingsRequest,
        openai_request: dict[str, Any],
        deployment: Deployment,
    ) -> tuple[openai.openai_object.OpenAIObject, int]:
        """call openai with retry"""

        url = (
            f"https://{deployment.resource_name}.openai.azure.com/openai/deployments/"
            f"{deployment.deployment_name}/embeddings"
            f"?api-version={model.api_version}"
        )

        async_mgr = OpenAIAsyncManager(deployment)
        response, http_status_code = await async_mgr.async_openai_post(openai_request, url)

        response["model"] = deployment.friendly_name

        return response, http_status_code
