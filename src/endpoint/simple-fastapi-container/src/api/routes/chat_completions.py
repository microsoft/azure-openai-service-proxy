""" chat completion routes """

from typing import AsyncGenerator
from fastapi import Request, Response, FastAPI
from fastapi.responses import StreamingResponse
import openai.openai_object


# pylint: disable=E0402
from .request_manager import RequestManager
from ..chat_completions import (
    ChatCompletionsRequest,
    ChatCompletions as RequestMgr,
)
from ..authorize import Authorize

from ..management import DeploymentClass


class ChatCompletions(RequestManager):
    """Completion route"""

    def __init__(
        self,
        *,
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
            deployment_class=DeploymentClass.OPENAI_CHAT.value,
            request_class_mgr=RequestMgr,
        )

        self.__include_router()

    def __include_router(self):
        """include router"""

        # Support for OpenAI SDK 0.28
        @self.router.post(
            "/engines/{engine_id}/chat/completions",
            status_code=200,
            response_model=None,
        )
        # Support for .NET Azure OpenAI Service SDK
        @self.router.post(
            "/openai/deployments/{deployment_id}/chat/completions",
            status_code=200,
            response_model=None,
        )
        # Support for .NET Azure OpenAI Extensions Chat Completions
        @self.router.post(
            "/openai/deployments/{deployment_id}/extensions/chat/completions",
            status_code=200,
            response_model=None,
        )
        # Support for Python Azure OpenAI SDK 1.0+
        @self.router.post(
            "/deployments/{deployment_id}/chat/completions",
            status_code=200,
            response_model=None,
        )
        # Support for OpenAI SDK 1.0+
        @self.router.post("/chat/completions", status_code=200, response_model=None)
        async def oai_chat_completion(
            chat: ChatCompletionsRequest,
            request: Request,
            response: Response,
            deployment_id: str = None,
        ) -> openai.openai_object.OpenAIObject | str | StreamingResponse:
            """OpenAI chat completion response"""

            # does the request have extensions in the path?
            if "extensions" in request.url.path:
                chat.extensions = True

            # get the api version from the query string
            if "api-version" in request.query_params:
                chat.api_version = request.query_params["api-version"]

            authorize_response = await self.authorize_request(
                deployment_id=deployment_id, request=request
            )

            if chat.max_tokens and chat.max_tokens > authorize_response.max_token_cap:
                chat.max_tokens = authorize_response.max_token_cap

            (
                completion,
                status_code,
            ) = await self.request_class_mgr.call_openai_chat_completion(chat)

            if isinstance(completion, AsyncGenerator):
                return StreamingResponse(completion)
            else:
                response.status_code = status_code
                return completion

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
