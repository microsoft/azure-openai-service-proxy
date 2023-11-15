""" OpenAI Async Manager"""

import json
import logging
import time
from typing import Tuple

import httpx
import openai
import openai.error
import openai.openai_object


from .config import Deployment

HTTPX_TIMEOUT_SECONDS = 30

logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)


class OpenAIException(Exception):
    """OpenAI Exception"""

    def __init__(self, message: str, status_code: int):
        """init"""
        super().__init__(message)
        self.http_status_code = status_code


class OpenAIAsyncManager:
    """OpenAI Manager"""

    def __init__(self, deployment: Deployment):
        """init in memory session manager"""
        self.deployment = deployment

    # retry strategy is fail fast
    # @retry(
    #     wait=wait_random(min=0, max=2),
    #     stop=stop_after_attempt(1),
    #     retry=retry_if_exception_type((OpenAIRetryException)),
    # )
    async def async_openai_post(
        self, openai_request: str, url: str
    ) -> Tuple[openai.openai_object.OpenAIObject, str]:
        """async openai post"""

        headers = {
            "Content-Type": "application/json",
            "api-key": self.deployment.endpoint_key,
        }

        start = time.time()

        async with httpx.AsyncClient() as client:
            response = await client.post(
                url,
                headers=headers,
                json=openai_request,
                timeout=HTTPX_TIMEOUT_SECONDS,
            )

        if not 200 <= response.status_code < 300:
            # note, for future reference 409 and 429 were retryable with tenactiy
            raise OpenAIException(
                message=json.loads(response.text)
                .get("error")
                .get("message", "OpenAI Error"),
                status_code=response.status_code,
            )

        # calculate response time in milliseconds
        end = time.time()
        response_ms = int((end - start) * 1000)

        try:
            openai_response = openai.openai_object.OpenAIObject.construct_from(
                json.loads(response.text), response_ms=response_ms
            )

        except Exception as exc:
            raise OpenAIException(
                message="Invalid response body from API", status_code=400
            ) from exc

        return openai_response

    async def async_post(self, openai_request: str, url: str):
        """async rest post"""

        headers = {
            "Content-Type": "application/json",
            "api-key": self.deployment.endpoint_key,
        }

        async with httpx.AsyncClient() as client:
            response = await client.post(
                url,
                headers=headers,
                json=openai_request,
                timeout=HTTPX_TIMEOUT_SECONDS,
            )

        response.raise_for_status()
        return response

    async def async_get(self, url: str):
        """async get request"""

        headers = {
            "Content-Type": "application/json",
            "api-key": self.deployment.endpoint_key,
        }

        async with httpx.AsyncClient() as client:
            response = await client.get(
                url,
                headers=headers,
                timeout=HTTPX_TIMEOUT_SECONDS,
            )

        response.raise_for_status()
        return response
