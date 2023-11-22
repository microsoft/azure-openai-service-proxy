""" dalle-3 and beyond """

from fastapi import APIRouter, Request, Response, FastAPI, HTTPException

# pylint: disable=E0402
from ..image_generation import ImagesGenerationsRequst


class ImagesGenerations:
    """Completion route"""

    def __init__(self, app: FastAPI, prefix: str, tags: list[str]):
        self.app = app
        self.router = APIRouter()
        self.prefix = prefix
        self.tags = tags
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
        # Support for OpenAI SDK 1.0+
        @self.router.post("/images/generations", status_code=200, response_model=None)
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
            _, user_token = await self.app.state.authorize.authorize_api_access(
                request.headers, deployment_id
            )

            if self.app.state.rate_limit_images_generations.is_call_rate_exceeded(
                user_token
            ):
                raise HTTPException(
                    status_code=429,
                    detail="Rate limit exceeded. Try again in 10 seconds",
                )

            (
                completion_response,
                status_code,
            ) = await self.app.state.images_generations_mgr.call_openai_images_generations(
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

            # exception thrown if not authorized
            await self.app.state.authorize.authorize_api_access(
                request.headers, deployment_id
            )

            if "api-version" in request.query_params:
                api_version = request.query_params["api-version"]

            (
                completion_response,
                status_code,
            ) = await self.app.state.images_generations_mgr.call_openai_images_get(
                friendly_name, image_id, api_version
            )
            response.status_code = status_code
            return completion_response

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
