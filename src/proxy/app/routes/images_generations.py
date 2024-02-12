""" dalle-2 """

from enum import Enum
from os import environ
from typing import Any

from fastapi import Request, Response
from pydantic import BaseModel

# pylint: disable=E0402
from ..authorize import AuthorizeResponse
from ..openai_async import OpenAIAsyncManager
from .request_manager import RequestManager


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


class ImagesGenerations(RequestManager):
    """Completion route"""

    def include_router(self):
        """include router"""

        # Support for Azure OpenAI Service SDK 1.0+
        @self.router.post(
            "/openai/images/generations:submit",
            status_code=200,
            response_model=None,
        )
        async def oai_images_generations(
            model: ImagesGenerationsRequst,
            request: Request,
            response: Response,
        ):
            """OpenAI image generation response"""

            # No deployment_is passed for images generation so set to dall-e
            deployment_name = "dalle-2"

            authorize_response = await self.authorize.authorize_azure_api_access(
                headers=request.headers, deployment_name=deployment_name
            )

            completion, status_code = await self.call_openai_images_generations(
                model, request, response, authorize_response
            )

            response.status_code = status_code

            return completion

        @self.router.get("/{deployment_name}/openai/operations/images/{image_id}")
        async def oai_images_get(
            deployment_name: str,
            image_id: str,
            request: Request,
            response: Response,
        ):
            """OpenAI image generation response"""

            authorize_response = await self.authorize_request(
                deployment_name=deployment_name, request=request
            )

            (
                completion_response,
                status_code,
            ) = await self.call_openai_images_get(deployment_name, image_id, authorize_response)

            response.status_code = status_code
            return completion_response

        return self.router

    async def call_openai_images_generations(
        self,
        images: ImagesGenerationsRequst,
        request: Request,
        response: Response,
        authorize_response: AuthorizeResponse,
    ) -> Any:
        """call openai with retry"""

        self.validate_input(images)

        deployment = await self.config.get_catalog_by_deployment_name(authorize_response)

        openai_request = {
            "prompt": images.prompt,
            "n": images.n,
            "size": images.size.value,
            "response_format": images.response_format.value,
        }

        url = (
            f"{deployment.endpoint_url}"
            "/openai/images/generations:submit"
            f"?api-version={self.api_version}"
        )

        async_mgr = OpenAIAsyncManager(deployment)
        dalle_response = await async_mgr.async_post(openai_request, url)

        if "operation-location" in dalle_response.headers:
            original_location = dalle_response.headers["operation-location"]
            port = f":{request.url.port}" if request.url.port else ""
            original_location_suffix = original_location.split("/openai", 1)[1]

            if environ.get("ENVIRONMENT") == "development":
                proxy_location = (
                    f"http://{request.url.hostname}{port}"
                    f"/api/v1/{deployment.deployment_name}/openai{original_location_suffix}"
                )
            else:
                proxy_location = (
                    f"https://{request.url.hostname}{port}"
                    f"/api/v1/{deployment.deployment_name}/openai{original_location_suffix}"
                )

            response.headers.append("operation-location", proxy_location)

        return dalle_response.json(), dalle_response.status_code

    async def call_openai_images_get(
        self, deployment_name: str, image_id: str, authorize_response: AuthorizeResponse
    ):
        """call openai with retry"""

        authorize_response.deployment_name = deployment_name

        deployment = await self.config.get_catalog_by_deployment_name(authorize_response)

        if deployment is None:
            return self.report_exception("Oops, failed to find service to generate image.", 404)

        url = (
            f"{deployment.endpoint_url}"
            f"/openai/operations/images/{image_id}"
            f"?api-version={self.api_version}"
        )

        async_mgr = OpenAIAsyncManager(deployment)
        dalle_response = await async_mgr.async_get(url)

        return dalle_response.json(), dalle_response.status_code

    def validate_input(self, images: ImagesGenerationsRequst):
        """validate input"""
        # do some basic input validation
        if not images.prompt:
            self.report_exception("Oops, no prompt.", 400)

        if len(images.prompt) > 1000:
            self.report_exception(
                "Oops, prompt is too long. The maximum length is 1000 characters.", 400
            )

        # check the image_count is between 1 and 5
        if images.n and not 1 <= images.n <= 5:
            self.report_exception("Oops, image_count must be between 1 and 5 inclusive.", 400)

        # check the image_size is between 256x256, 512x512, 1024x1024
        if images.size and images.size not in ImageSize:
            self.report_exception("Oops, image_size must be 256x256, 512x512, 1024x1024.", 400)

        # check the response_format is url or base64
        if images.response_format and images.response_format not in ResponseFormat:
            self.report_exception("Oops, response_format must be url or b64_json.", 400)
