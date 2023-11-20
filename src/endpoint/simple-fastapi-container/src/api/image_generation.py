""" Images Generations API """

from enum import Enum
import logging
from typing import Tuple
import openai
from pydantic import BaseModel
from fastapi import HTTPException, Request, Response

from .configuration import Deployment, OpenAIConfig
from .openai_async import OpenAIAsyncManager

OPENAI_IMAGES_GENERATIONS_API_VERSION = "2023-06-01-preview"

logging.basicConfig(level=logging.WARNING)


class ResponseFormat(Enum):
    """Response Format"""

    URL = "url"
    BASE64 = "b64_json"


class ImageSize(Enum):
    """Image Size"""

    IS_256X256 = "256x256"
    IS_512X512 = "512x512"
    IS_1024X1024 = "1024x1024"


class DalleTimeoutError(Exception):
    """Raised when the Dalle request times out"""


class ImagesGenerationsRequst(BaseModel):
    """OpenAI Images Generations Request"""

    prompt: str
    response_format: ResponseFormat = ResponseFormat.URL
    n: int = 1
    size: ImageSize = ImageSize.IS_1024X1024
    user: str = None
    api_version: str = OPENAI_IMAGES_GENERATIONS_API_VERSION


class ImagesGenerations:
    """OpenAI Images Generations Manager"""

    def __init__(self, openai_config: OpenAIConfig):
        """init in memory session manager"""
        self.openai_config = openai_config
        self.logger = logging.getLogger(__name__)

    def report_exception(
        self, message: str, http_status_code: int
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """report exception"""

        self.logger.warning(msg=f"{message}")

        raise HTTPException(
            status_code=http_status_code,
            detail=message,
        )

    def validate_input(self, images: ImagesGenerationsRequst):
        """validate input"""
        # do some basic input validation
        if not images.prompt:
            return self.report_exception("Oops, no prompt.", 400)

        if len(images.prompt) > 1000:
            return self.report_exception(
                "Oops, prompt is too long. The maximum length is 1000 characters.", 400
            )

        # check the image_count is between 1 and 5
        if images.n and not 1 <= images.n <= 5:
            return self.report_exception(
                "Oops, image_count must be between 1 and 5 inclusive.", 400
            )

        # check the image_size is between 256x256, 512x512, 1024x1024
        if images.size and images.size not in ImageSize:
            return self.report_exception(
                "Oops, image_size must be 256x256, 512x512, 1024x1024.", 400
            )

        # check the response_format is url or base64
        if images.response_format and images.response_format not in ResponseFormat:
            return self.report_exception(
                "Oops, response_format must be url or b64_json.", 400
            )

    async def call_openai_images_generations(
        self, images: ImagesGenerationsRequst, request: Request, response: Response
    ) -> Tuple[Deployment, openai.openai_object.OpenAIObject, int]:
        """call openai with retry"""

        self.validate_input(images)

        deployment = await self.openai_config.get_deployment()

        openai_request = {
            "prompt": images.prompt,
            "n": images.n,
            "size": images.size.value,
            "response_format": images.response_format.value,
        }

        url = (
            f"https://{deployment.resource_name}.openai.azure.com"
            "/openai/images/generations:submit"
            f"?api-version={images.api_version}"
        )

        async_mgr = OpenAIAsyncManager(deployment)
        dalle_response = await async_mgr.async_post(openai_request, url)

        for value in dalle_response.headers:
            if value == "operation-location":
                original_location = dalle_response.headers[value]
                port = f":{request.url.port}" if request.url.port else ""
                original_location_suffix = original_location.split("/openai", 1)[1]

                proxy_location = (
                    f"{request.url.scheme}://{request.url.hostname}{port}"
                    f"/v1/api/{deployment.friendly_name}/openai{original_location_suffix}"
                )
                response.headers.append(value, proxy_location)
            else:
                response.headers.append(value, dalle_response.headers[value])

        return dalle_response.json(), dalle_response.status_code

    async def call_openai_images_get(
        self,
        friendly_name: str,
        image_id: str,
        api_version: str = OPENAI_IMAGES_GENERATIONS_API_VERSION,
    ):
        """call openai with retry"""

        deployment = await self.openai_config.get_deployment_by_friendly_name(
            friendly_name
        )

        if deployment is None:
            return self.report_exception(
                "Oops, failed to find service to generate image.", 404
            )

        url = (
            f"https://{deployment.resource_name}.openai.azure.com"
            f"/openai/operations/images/{image_id}"
            f"?api-version={api_version}"
        )

        async_mgr = OpenAIAsyncManager(deployment)
        dalle_response = await async_mgr.async_get(url)

        return dalle_response.json(), dalle_response.status_code
