""" chat completion routes """

from collections.abc import AsyncGenerator
from typing import Any

import openai.openai_object
from fastapi import Request, Response
from fastapi.responses import StreamingResponse
from pydantic import BaseModel

# pylint: disable=E0402
from ..config import Deployment
from ..openai_async import OpenAIAsyncManager
from .request_manager import RequestManager

OPENAI_CHAT_COMPLETIONS_EXTENSIONS_API_VERSION = "2023-08-01-preview"


class ChatExtensionsRequest(BaseModel):
    """OpenAI Chat Request"""

    messages: list[dict[str, str]]
    dataSources: list[Any] | None = None
    max_tokens: int = None
    temperature: float = None
    n: int | None = None
    stream: bool = False
    top_p: float | None = None
    stop: str | list[str] | None = None
    frequency_penalty: float | None = None
    presence_penalty: float | None = None
    functions: list[dict[str, Any]] | None = None
    function_call: str | dict[str, str] | None = None
    api_version: str = OPENAI_CHAT_COMPLETIONS_EXTENSIONS_API_VERSION


class ChatExtensions(RequestManager):
    """Completion route"""

    def include_router(self):
        """include router"""

        # Support for .NET Azure OpenAI Extensions Chat Completions
        @self.router.post(
            "/openai/deployments/{deployment_id}/extensions/chat/completions",
            status_code=200,
            response_model=None,
        )
        async def oai_chat_completion(
            model: ChatExtensionsRequest,
            request: Request,
            response: Response,
            deployment_id: str = None,
        ) -> openai.openai_object.OpenAIObject | str | StreamingResponse:
            """OpenAI chat completion response"""

            if model.api_version is None:
                model.api_version = OPENAI_CHAT_COMPLETIONS_EXTENSIONS_API_VERSION

            completion, status_code = await self.process_request(
                deployment_id=deployment_id,
                request=request,
                model=model,
                call_method=self.call_openai,
                validate_method=self.__validate_chat_extensions_request,
            )

            if isinstance(completion, AsyncGenerator):
                return StreamingResponse(completion)

            response.status_code = status_code
            return completion

        return self.router

    async def call_openai(
        self,
        model: ChatExtensionsRequest,
        openai_request: dict[str, Any],
        deployment: Deployment,
    ) -> tuple[openai.openai_object.OpenAIObject, int] | AsyncGenerator:
        """call openai with retry"""

        url = (
            f"https://{deployment.resource_name}.openai.azure.com/openai/deployments/"
            f"{deployment.deployment_name}/extensions/chat/completions"
            f"?api-version={model.api_version}"
        )

        async_mgr = OpenAIAsyncManager(deployment)

        if model.stream:
            response = await async_mgr.async_post_streaming(openai_request, url)
        else:
            response, http_status_code = await async_mgr.async_openai_post(openai_request, url)

        return response, http_status_code

    def __validate_chat_extensions_request(self, model: ChatExtensionsRequest):
        """validate input"""

        # check the max_tokens is between 1 and 4000
        if model.max_tokens is not None and not 1 <= model.max_tokens <= 4000:
            self.report_exception("Oops, max_tokens must be between 1 and 4000.", 400)

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
