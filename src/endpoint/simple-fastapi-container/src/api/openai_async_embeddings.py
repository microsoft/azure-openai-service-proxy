""" OpenAI Async Manager"""

import json
import logging
import time
from typing import Any, Tuple

import httpx
import openai
import openai.error
import openai.openai_object
from fastapi import FastAPI
from pydantic import BaseModel
from tenacity import retry, stop_after_attempt, wait_random, retry_if_exception_type

from .config_embeddings import OpenAIConfig

HTTPX_TIMEOUT_SECONDS = 30

logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)


class EmbeddingsRequest(BaseModel):
    """OpenAI Chat Request"""

    input: str
    model: str = ""


class OpenAIAsyncManager:
    """OpenAI Manager"""

    def __init__(self, app: FastAPI, openai_config: OpenAIConfig):
        """init in memory session manager"""
        self.openai_config = openai_config
        self.app = app

    # retry strategy is fail fast
    @retry(
        wait=wait_random(min=0, max=2),
        stop=stop_after_attempt(1),
        retry=retry_if_exception_type(
            (openai.error.TryAgain, openai.error.RateLimitError)
        ),
    )
    async def get_openai_embeddings(
        self, embedding: EmbeddingsRequest
    ) -> Tuple[openai.openai_object.OpenAIObject, str]:
        """async get openai completion"""

        def get_error(response: httpx.Response) -> dict[str, dict[str, Any]]:
            """get error message from response"""
            try:
                return json.loads(response.text).get("error")
            except json.JSONDecodeError:
                return {}

        deployment = await self.openai_config.get_deployment()

        deployment_name = deployment.deployment_name
        endpoint_key = deployment.endpoint_key
        model = deployment.deployment_name
        friendly_name = deployment.friendly_name
        resource_name = deployment.resource_name
        api_version = "2023-08-01-preview"

        openai_request = {
            "input": embedding.input,
            "model": model,
        }

        url = (
            f"https://{resource_name}.openai.azure.com/openai/deployments/"
            f"{deployment_name}/embeddings?api-version={api_version}"
        )

        headers = {
            "Content-Type": "application/json",
            "api-key": endpoint_key,
        }

        start = time.time()

        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    url,
                    headers=headers,
                    json=openai_request,
                    timeout=HTTPX_TIMEOUT_SECONDS,
                )

            if not 200 <= response.status_code < 300:
                error = get_error(response)

                if response.status_code in [400, 404, 415]:
                    message = error.get("message", "Bad request")
                    param = error.get("param")
                    code = error.get("code")

                    raise openai.error.InvalidRequestError(
                        message=message,
                        param=param,
                        code=code,
                        http_status=response.status_code,
                    )

                elif response.status_code == 401:
                    message = error.get("message", "Unauthorized")

                    raise openai.error.AuthenticationError(
                        message=message, http_status=response.status_code
                    )

                elif response.status_code == 403:
                    message = error.get("message", "Permission Error")

                    raise openai.error.PermissionError(
                        message=message, http_status=response.status_code
                    )

                elif response.status_code == 409:
                    message = error.get("message", "Try Again")

                    raise openai.error.TryAgain(
                        message=message, http_status=response.status_code
                    )

                elif response.status_code == 429:
                    message = error.get("message", "Rate limited.")

                    raise openai.error.RateLimitError(
                        message=message,
                        http_status=response.status_code,
                    )
                else:
                    message = error.get("message", "OpenAI Error")

                    raise openai.error.APIError(message=message)

        except httpx.ConnectError as connect_error:
            raise openai.error.ServiceUnavailableError(
                "Service unavailable"
            ) from connect_error

        except httpx.ConnectTimeout as connect_timeout:
            raise openai.error.Timeout("Timeout error") from connect_timeout

        # calculate response time in milliseconds
        end = time.time()
        response_ms = int((end - start) * 1000)

        try:
            openai_response = openai.openai_object.OpenAIObject.construct_from(
                json.loads(response.text), response_ms=response_ms
            )

        except Exception as exc:
            raise openai.APIError(
                f"Invalid response body from API: {response.text}"
            ) from exc

        return openai_response, friendly_name
