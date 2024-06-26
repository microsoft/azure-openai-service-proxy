""" chat completion routes """

from collections.abc import AsyncGenerator
from typing import Any

from app.config import Deployment
from app.routes.request_manager import RequestManager
from fastapi import Request, Response
from fastapi.responses import StreamingResponse
from pydantic import BaseModel

# pylint: disable=E0402
from ..openai_async import OpenAIAsyncManager


class ChatCompletionsRequest(BaseModel):
    """OpenAI Chat Request"""

    messages: list[Any] | None = None
    dataSources: list[Any] | None = None
    enhancements: list[Any] | None = None
    max_tokens: int | None = None
    temperature: float | None = None
    n: int | None = None
    stream: bool = False
    top_p: float | None = None
    stop: str | list[str] | None = None
    frequency_penalty: float | None = None
    presence_penalty: float | None = None
    functions: list[dict[str, Any]] | None = None
    function_call: str | dict[str, str] | None = None
    tools: Any | None = None
    tools_choice: Any | None = None


class ChatCompletions(RequestManager):
    """Completion route"""

    def include_router(self):
        """include router"""

        # Support for Python Azure OpenAI SDK 1.0+
        @self.router.post(
            "/deployments/{deployment_name}/chat/completions",
            status_code=200,
            response_model=None,
        )
        # Support for .NET Azure OpenAI Service SDK
        @self.router.post(
            "/openai/deployments/{deployment_name}/chat/completions",
            status_code=200,
            response_model=None,
        )
        # Support for .NET Azure OpenAI Extensions Chat Completions
        @self.router.post(
            "/openai/deployments/{deployment_name}/extensions/chat/completions",
            status_code=200,
            response_model=None,
        )
        async def oai_chat_completion(
            model: ChatCompletionsRequest,
            request: Request,
            response: Response,
            deployment_name: str = None,
        ) -> Any:
            """OpenAI chat completion response"""

            completion, status_code = await self.process_request(
                deployment_name=deployment_name,
                request=request,
                model=model,
                call_method=self.call_openai_chat,
                validate_method=self.__validate_chat_completion_request,
            )

            if isinstance(completion, AsyncGenerator):
                return StreamingResponse(completion)

            response.status_code = status_code
            return completion

        return self.router

    async def call_openai_chat(
        self,
        model: object,
        deployment: Deployment,
        request: Request,
    ) -> Any:
        """call openai with retry"""

        if "extension" in request.url.path:
            url = (
                f"{deployment.endpoint_url}/openai/deployments/"
                f"{deployment.deployment_name}/extensions/chat/completions"
                f"?api-version={self.api_version}"
            )
        else:
            url = (
                f"{deployment.endpoint_url}/openai/deployments/"
                f"{deployment.deployment_name}/chat/completions"
                f"?api-version={self.api_version}"
            )

        openai_request = self.model_to_dict(model)
        async_mgr = OpenAIAsyncManager(deployment)

        if model.stream:
            response, http_status_code = await async_mgr.async_post_streaming(openai_request, url)
        else:
            response, http_status_code = await async_mgr.async_openai_post(openai_request, url)

        return response, http_status_code

    def __validate_chat_completion_request(self, model: ChatCompletionsRequest):
        """validate input"""

        if model.n is not None and not 1 <= model.n <= 10:
            self.throw_validation_error("Oops, n must be between 1 and 10.", 400)

        # check the temperature is between 0 and 1
        if model.temperature is not None and not 0 <= model.temperature <= 1:
            self.report_exception("Oops, temperature must be between 0 and 1.", 400)

        # check the top_p is between 0 and 1
        if model.top_p is not None and not 0 <= model.top_p <= 1:
            self.report_exception("Oops, top_p must be between 0 and 1.", 400)

        # check the frequency_penalty is between 0 and 1
        if model.frequency_penalty is not None and not 0 <= model.frequency_penalty <= 1:
            self.report_exception("Oops, frequency_penalty must be between 0 and 1.", 400)

        # check the presence_penalty is between 0 and 1
        if model.presence_penalty is not None and not 0 <= model.presence_penalty <= 1:
            self.report_exception("Oops, presence_penalty must be between 0 and 1.", 400)
