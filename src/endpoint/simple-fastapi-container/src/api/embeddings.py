""" OpenAI Embeddings Manager """

from typing import Tuple
import logging
import httpx
import openai
import openai.error
import openai.openai_object
from pydantic import BaseModel

from .openai_async import OpenAIAsyncManager, OpenAIException
from .config import OpenAIConfig

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

    def report_exception(
        self, message: str, http_status_code: int
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """report exception"""

        self.logger.warning(msg=f"{message}")

        return message, http_status_code

    async def call_openai_embeddings(
        self, embedding: EmbeddingsRequest
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """call openai with retry"""

        try:
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
            response = await async_mgr.async_openai_post(openai_request, url)

            response["model"] = deployment.friendly_name

            return response, 200

        except httpx.ConnectError:
            return self.report_exception(
                "Service connection error.",
                504,
            )

        except httpx.ConnectTimeout:
            return self.report_exception(
                "Service connection timeout error.",
                504,
            )

        except OpenAIException as openai_exception:
            return self.report_exception(
                openai_exception.args[0],
                openai_exception.http_status_code,
            )

        except Exception as exception:
            self.logger.warning(msg=f"Global exception caught: {exception}")
            raise exception
