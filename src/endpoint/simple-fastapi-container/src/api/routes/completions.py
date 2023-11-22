""" completion routes """

from fastapi import APIRouter, Request, Response, HTTPException, FastAPI
import openai.openai_object
from src.api.completions import CompletionsRequest


class Completions:
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

            if (
                completion_request.max_tokens
                and completion_request.max_tokens > authorize_response.max_token_cap
            ):
                completion_request.max_tokens = authorize_response.max_token_cap

            (
                completion_response,
                status_code,
            ) = await self.app.state.completions_mgr.call_openai_completion(
                completion_request
            )
            response.status_code = status_code
            return completion_response

        self.app.include_router(self.router, prefix=self.prefix, tags=self.tags)
