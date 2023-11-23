""" dalle-3 and beyond """

from fastapi import Request, Response, FastAPI


# pylint: disable=E0402
from .request_manager import RequestManager
from ..image_generation import (
    ImagesGenerationsRequst,
    ImagesGenerations as RequestMgr,
)
from ..authorize import Authorize
from ..management import DeploymentClass


class ImagesGenerations(RequestManager):
    """Completion route"""

    def __init__(
        self,
        app: FastAPI,
        authorize: Authorize,
        connection_string: str,
        prefix: str,
        tags: list[str],
    ):
        super().__init__(
            app=app,
            authorize=authorize,
            connection_string=connection_string,
            prefix=prefix,
            tags=tags,
            deployment_class=DeploymentClass.OPENAI_IMAGES_GENERATIONS.value,
            request_class_mgr=RequestMgr,
        )

        self.__include_router()

    def __include_router(self):
        """include router"""

        # Support for Dall-e-2
        # Support for OpenAI SDK 0.28
        @self.router.post(
            "/engines/{engine_id}/images/generations",
            status_code=200,
            response_model=None,
        )
        # Support for Azure OpenAI Service SDK 1.0+
        @self.router.post(
            "/openai/images/generations:submit",
            status_code=200,
            response_model=None,
        )
        async def oai_images_generations(
            image_generation_request: ImagesGenerationsRequst,
            request: Request,
            response: Response,
        ):
            """OpenAI image generation response"""

            # No deployment_is passed for images generation so set to dall-e
            deployment_id = "dall-e"

            # get the api version from the query string
            if "api-version" in request.query_params:
                image_generation_request.api_version = request.query_params[
                    "api-version"
                ]

            # exception thrown if not authorized
            await self.authorize_request(deployment_id=deployment_id, request=request)

            (
                completion_response,
                status_code,
            ) = await self.request_class_mgr.call_openai_images_generations(
                image_generation_request, request, response
            )
            response.status_code = status_code

            return completion_response

        @self.router.get("/{friendly_name}/openai/operations/images/{image_id}")
        async def oai_images_get(
            friendly_name: str,
            image_id: str,
            request: Request,
            response: Response,
        ):
            """OpenAI image generation response"""

            # No deployment_is passed for images generation so set to dall-e
            deployment_id = "dall-e"

            if "api-version" in request.query_params:
                api_version = request.query_params["api-version"]

            await self.authorize_request(deployment_id=deployment_id, request=request)

            (
                completion_response,
                status_code,
            ) = await self.request_class_mgr.call_openai_images_get(
                friendly_name, image_id, api_version
            )
            response.status_code = status_code
            return completion_response

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
