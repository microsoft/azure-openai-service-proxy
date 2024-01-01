""" dalle-3 and beyond """

from enum import Enum

import openai.openai_object
from fastapi import Request, Response
from pydantic import BaseModel

# pylint: disable=E0402
from ..config import Deployment
from ..openai_async import OpenAIAsyncManager
from .request_manager import RequestManager


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


class Images(RequestManager):
    """Completion route"""

    def include_router(self):
        """include router"""

        # Azure OpenAI Images
        @self.router.post(
            "/openai/deployments/{deployment_name}/images/generations",
            status_code=200,
            response_model=None,
        )
        async def oai_images(
            model: ImagesRequest,
            request: Request,
            response: Response,
            deployment_name: str = None,
        ):
            """OpenAI image generation response"""

            completion, status_code = await self.process_request(
                deployment_name=deployment_name,
                request=request,
                model=model,
                call_method=self.call_openai_images_generations,
                validate_method=self.__validate_image_request,
            )

            response.status_code = status_code
            return completion

        return self.router

    async def call_openai_images_generations(
        self,
        model: object,
        deployment: Deployment,
    ) -> tuple[openai.openai_object.OpenAIObject, int]:
        """call openai with retry"""

        openai_request = {
            "prompt": model.prompt,
            "size": model.size.value,
            "n": model.n,
            "quality": model.quality.value,
            "style": model.style.value,
        }

        url = (
            f"https://{deployment.resource_name}.openai.azure.com/openai/deployments/"
            f"{deployment.deployment_name}/images/generations"
            f"?api-version={self.api_version}"
        )

        async_mgr = OpenAIAsyncManager(deployment)
        response = await async_mgr.async_post(openai_request, url)

        response_json = response.json()

        return response_json, response.status_code

    def __validate_image_request(self, images: ImagesRequest):
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
            return self.report_exception("Oops, image_quality must be hd, standard.", 400)

        if images.style and images.style not in ImageStyle:
            return self.report_exception("Oops, image_style must be vivid, natural.", 400)
