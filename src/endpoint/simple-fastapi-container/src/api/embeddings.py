""" OpenAI Embeddings Manager """

from typing import Tuple
import logging
import openai
import openai.error
import openai.openai_object
from pydantic import BaseModel

from .openai_async import OpenAIAsyncManager
from .configuration import OpenAIConfig

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

    async def call_openai_embeddings(
        self, embedding: EmbeddingsRequest
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
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
        [response, status_code] = await async_mgr.async_openai_post(openai_request, url)

        response["model"] = deployment.friendly_name

        return response, status_code
