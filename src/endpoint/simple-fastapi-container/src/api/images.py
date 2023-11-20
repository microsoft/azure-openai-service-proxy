""" Images Generations API for Dall-e models"""

from enum import Enum
import logging
from typing import Tuple
import openai
from pydantic import BaseModel
from fastapi import HTTPException

from .configuration import OpenAIConfig
from .openai_async import OpenAIAsyncManager

OPENAI_IMAGES_GENERATIONS_API_VERSION = "2023-12-01-preview"

logging.basicConfig(level=logging.WARNING)


class ResponseFormat(Enum):
    """Response Format"""

    URL = "url"
    BASE64 = "b64_json"


class ImageSize(Enum):
    """Image Size"""

    IS_1024X1024 = "1024x1024"
    IS_1792X1024 = "1792x1024"
    IS_1024X1792 = "1024x1792"


class ImageQuality(Enum):
    """Image Quality"""

    HD = "hd"
    STANDARD = "standard"


class ImageStyle(Enum):
    """Image Style"""

    VIVID = "vivid"
    NATURAL = "natural"


class ImagesRequest(BaseModel):
    """OpenAI Images Generations Request"""

    prompt: str
    # response_format: ResponseFormat = ResponseFormat.URL
    n: int = 1
    size: ImageSize = ImageSize.IS_1024X1024
    quality: ImageQuality = ImageQuality.HD
    style: ImageStyle = ImageStyle.VIVID
    api_version: str = OPENAI_IMAGES_GENERATIONS_API_VERSION


class Images:
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

    def validate_input(self, images: ImagesRequest):
        """validate input"""
        # do some basic input validation
        if not images.prompt:
            return self.report_exception("Oops, no prompt.", 400)

        if len(images.prompt) > 1000:
            return self.report_exception(
                "Oops, prompt is too long. The maximum length is 1000 characters.", 400
            )

        # check the image_count is 1
        if images.n and images.n != 1:
            return self.report_exception("Oops, image_count must be 1.", 400)

        # check the image_size is between 256x256, 512x512, 1024x1024
        if images.size and images.size not in ImageSize:
            return self.report_exception(
                "Oops, image_size must be 1792x1024, 1024x1792, 1024x1024.", 400
            )

        if images.quality and images.quality not in ImageQuality:
            return self.report_exception(
                "Oops, image_quality must be hd, standard.", 400
            )

        if images.style and images.style not in ImageStyle:
            return self.report_exception(
                "Oops, image_style must be vivid, natural.", 400
            )

    async def call_openai_images_generations(
        self, images: ImagesRequest
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """call openai with retry"""

        self.validate_input(images)

        deployment = await self.openai_config.get_deployment()

        openai_request = {
            "prompt": images.prompt,
            "size": images.size.value,
            "n": images.n,
            "quality": images.quality.value,
            "style": images.style.value,
        }

        url = (
            f"https://{deployment.resource_name}.openai.azure.com/openai/deployments/"
            f"{deployment.deployment_name}/images/generations"
            f"?api-version={images.api_version}"
        )

        async_mgr = OpenAIAsyncManager(deployment)
        response = await async_mgr.async_post(openai_request, url)

        response_json = response.json()

        return response_json, response.status_code
