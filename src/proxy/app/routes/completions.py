""" completion routes """

from typing import Any

from fastapi import Request, Response
from pydantic import BaseModel

# pylint: disable=E0402
from ..config import Deployment
from ..openai_async import OpenAIAsyncManager
from .request_manager import RequestManager


class CompletionsRequest(BaseModel):
    """OpenAI Compeletion Request"""

    prompt: str | list[str]
    max_tokens: int | None = None
    temperature: float | None = None
    top_p: float | None = None
    stop: Any | None = None
    frequency_penalty: float = 0
    presence_penalty: float = 0


class Completions(RequestManager):
    """Completion route"""

    def include_router(self):
        """include router"""

        # Support for Azure OpenAI Service SDK 1.0+
        @self.router.post(
            "/openai/deployments/{deployment_name}/completions",
            status_code=200,
            response_model=None,
        )
        async def oai_completion(
            model: CompletionsRequest,
            request: Request,
            response: Response,
            deployment_name: str = None,
        ) -> Any:
            """OpenAI completion response"""

            completion, status_code = await self.process_request(
                deployment_name=deployment_name,
                request=request,
                model=model,
                call_method=self.call_openai,
                validate_method=self.__validate_completion_request,
            )

            response.status_code = status_code
            return completion

        return self.router

    async def call_openai(
        self,
        model: object,
        deployment: Deployment,
    ) -> Any:
        """call openai with retry"""

        url = (
            f"{deployment.endpoint_url}/openai/deployments/"
            f"{deployment.deployment_name}/completions"
            f"?api-version={self.api_version}"
        )

        openai_request = self.model_to_dict(model)
        async_mgr = OpenAIAsyncManager(deployment)
        response, http_status_code = await async_mgr.async_openai_post(openai_request, url)

        return response, http_status_code

    def __validate_completion_request(self, model: CompletionsRequest):
        if not model.prompt:
            self.report_exception("Oops, no prompt.", 400)
