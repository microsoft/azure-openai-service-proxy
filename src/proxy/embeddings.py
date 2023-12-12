""" OpenAI Embeddings Manager """

import logging
from typing import Any

from pydantic import BaseModel

from .configuration import OpenAIConfig
from .openai_async import OpenAIAsyncManager

OPENAI_EMBEDDINGS_API_VERSION = "2023-08-01-preview"

logging.basicConfig(level=logging.WARNING)


class EmbeddingsRequest(BaseModel):
    """OpenAI Chat Request"""

    input: str | list[str]
    model: str = ""
    api_version: str = OPENAI_EMBEDDINGS_API_VERSION


class Embeddings:
    """OpenAI Embeddings Manager"""

    def __init__(self, openai_config: OpenAIConfig):
        """init in memory session manager"""

        self.openai_config = openai_config
        self.logger = logging.getLogger(__name__)

    async def call_openai_embeddings(self, embedding: EmbeddingsRequest) -> Any:
        """call openai with retry"""

        deployment = await self.openai_config.get_deployment()

        openai_request = {
            "input": embedding.input,
            "model": deployment.deployment_name,
        }

        url = (
            f"https://{deployment.resource_name}.openai.azure.com/openai/deployments/"
            f"{deployment.deployment_name}/embeddings"
            f"?api-version={embedding.api_version}"
        )

        async_mgr = OpenAIAsyncManager(deployment)
        (response, status_code) = await async_mgr.async_openai_post(openai_request, url)

        response["model"] = deployment.friendly_name

        return response, status_code
