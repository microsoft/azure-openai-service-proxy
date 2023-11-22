""" chat completion routes """

from typing import AsyncGenerator
from fastapi import APIRouter, Request, Response, HTTPException, FastAPI
from fastapi.responses import StreamingResponse
import openai.openai_object

# pylint: disable=E0402
from ..chat_completions import ChatCompletionsRequest


class ChatCompletions:
    """Completion route"""

    def __init__(self, app: FastAPI, prefix: str, tags: list[str]):
        self.app = app
        self.router = APIRouter()
        self.prefix = prefix
        self.tags = tags
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

            # get the api version from the query string
            if "api-version" in request.query_params:
                chat.api_version = request.query_params["api-version"]

            # exception thrown if not authorized
            (
                authorize_response,
                user_token,
            ) = await self.app.state.authorize.authorize_api_access(
                request.headers, deployment_id
            )

            if self.app.state.rate_limit_chat_completion.is_call_rate_exceeded(
                user_token
            ):
                raise HTTPException(
                    status_code=429,
                    detail="Rate limit exceeded. Try again in 10 seconds",
                )

            if chat.max_tokens and chat.max_tokens > authorize_response.max_token_cap:
                chat.max_tokens = authorize_response.max_token_cap

            (
                completion,
                status_code,
            ) = await self.app.state.chat_completions_mgr.call_openai_chat_completion(
                chat
            )

            if isinstance(completion, AsyncGenerator):
                return StreamingResponse(completion)
            else:
                response.status_code = status_code
                return completion

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
