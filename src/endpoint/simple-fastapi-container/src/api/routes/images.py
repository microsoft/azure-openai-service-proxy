""" dalle-3 and beyond """

from fastapi import APIRouter, Request, Response, FastAPI

# pylint: disable=E0402
from ..images import ImagesRequest


class Images:
    """Completion route"""

    def __init__(self, app: FastAPI, prefix: str, tags: list[str]):
        self.app = app
        self.router = APIRouter()
        self.prefix = prefix
        self.tags = tags
        self.__include_router()

    def __include_router(self):
        """include router"""

        # Support for Dall-e-3 and beyond
        # Azure OpenAI Images
        @self.router.post(
            "/openai/deployments/{deployment_id}/images/generations",
            status_code=200,
            response_model=None,
        )
        # OpenAI Images
        @self.router.post(
            "/images/generations",
            status_code=200,
            response_model=None,
        )
        async def oai_images(
            images_request: ImagesRequest,
            request: Request,
            response: Response,
            deployment_id: str = None,
        ):
            """OpenAI image generation response"""

            # get the api version from the query string
            if "api-version" in request.query_params:
                images_request.api_version = request.query_params["api-version"]

            # exception thrown if not authorized
            await self.app.state.authorize.authorize_api_access(
                request.headers, deployment_id
            )

            (
                completion_response,
                status_code,
            ) = await self.app.state.images_mgr.call_openai_images_generations(
                images_request
            )
            response.status_code = status_code
            return completion_response

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
