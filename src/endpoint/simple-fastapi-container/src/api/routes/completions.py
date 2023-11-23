""" completion routes """

from fastapi import Request, Response, FastAPI
import openai.openai_object

# pylint: disable=E0402
from .request_manager import RequestManager
from ..completions import CompletionsRequest, Completions as RequestMgr
from ..authorize import Authorize
from ..management import DeploymentClass


class Completions(RequestManager):
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
            deployment_class=DeploymentClass.OPENAI_COMPLETIONS.value,
            request_class_mgr=RequestMgr,
        )

        self.__include_router()

    def __include_router(self):
        """include router"""

        # Support for OpenAI SDK 0.28
        @self.router.post(
            "/engines/{engine_id}/completions",
            status_code=200,
            response_model=None,
        )
        # Support for Azure OpenAI Service SDK 1.0+
        @self.router.post(
            "/openai/deployments/{deployment_id}/completions",
            status_code=200,
            response_model=None,
        )
        # Support for OpenAI SDK 1.0+
        @self.router.post("/completions", status_code=200, response_model=None)
        async def oai_completion(
            completion_request: CompletionsRequest,
            request: Request,
            response: Response,
            deployment_id: str = None,
        ) -> openai.openai_object.OpenAIObject | str:
            """OpenAI completion response"""

            # get the api version from the query string
            if "api-version" in request.query_params:
                completion_request.api_version = request.query_params["api-version"]

            # exception thrown if not authorized
            authorize_response = await self.authorize_request(
                deployment_id=deployment_id, request=request
            )

            if (
                completion_request.max_tokens
                and completion_request.max_tokens > authorize_response.max_token_cap
            ):
                completion_request.max_tokens = authorize_response.max_token_cap

            (
                completion_response,
                status_code,
            ) = await self.request_class_mgr.call_openai_completion(completion_request)

            response.status_code = status_code
            return completion_response

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
