""" OpenAI Async Manager"""

import json
import logging
import time
from typing import Tuple
from fastapi import HTTPException

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

        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    url,
                    headers=headers,
                    json=openai_request,
                    timeout=HTTPX_TIMEOUT_SECONDS,
                )

            if not 200 <= response.status_code < 300:
                # note, for future reference 409 and 429 were retryable with tenactiy
                raise HTTPException(
                    status_code=response.status_code,
                    detail=json.loads(response.text)
                    .get("error")
                    .get("message", "OpenAI Error"),
                )

        except HTTPException:
            raise

        except httpx.ConnectError as exc:
            raise HTTPException(
                status_code=504,
                detail="Service connection error.",
            ) from exc

        except httpx.ConnectTimeout as exc:
            raise HTTPException(
                status_code=504,
                detail="Service connection timeout error.",
            ) from exc

        except Exception as exc:
            raise HTTPException(
                status_code=500,
                detail="OpenAI API call failed",
            ) from exc

        # calculate response time in milliseconds
        end = time.time()
        response_ms = int((end - start) * 1000)

        try:
            openai_response = openai.openai_object.OpenAIObject.construct_from(
                json.loads(response.text), response_ms=response_ms
            )

        except Exception as exc:
            raise HTTPException(
                status_code=400,
                detail="Invalid response body from API",
            ) from exc

        return openai_response

    async def async_post(self, openai_request: str, url: str):
        """async rest post"""

        headers = {
            "Content-Type": "application/json",
            "api-key": self.deployment.endpoint_key,
        }

        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    url,
                    headers=headers,
                    json=openai_request,
                    timeout=HTTPX_TIMEOUT_SECONDS,
                )

            response.raise_for_status()
            return response

        except httpx.HTTPStatusError as http_status_error:
            raise HTTPException(
                status_code=http_status_error.response.status_code,
                detail=json.loads(http_status_error.response.text)
                .get("error")
                .get("message", "OpenAI Error"),
            ) from http_status_error

        except httpx.ConnectError as exc:
            raise HTTPException(
                status_code=504,
                detail="Service connection error.",
            ) from exc

        except httpx.ConnectTimeout as exc:
            raise HTTPException(
                status_code=504,
                detail="Service connection timeout error.",
            ) from exc

        except Exception as exc:
            raise HTTPException(
                status_code=500,
                detail="OpenAI API call failed",
            ) from exc

    async def async_get(self, url: str):
        """async get request"""

        headers = {
            "Content-Type": "application/json",
            "api-key": self.deployment.endpoint_key,
        }

        try:
            async with httpx.AsyncClient() as client:
                response = await client.get(
                    url,
                    headers=headers,
                    timeout=HTTPX_TIMEOUT_SECONDS,
                )

            response.raise_for_status()
            return response

        except httpx.HTTPStatusError as http_status_error:
            detail = (
                json.loads(http_status_error.response.text)
                .get("error")
                .get("message", "OpenAI Error")
            )

            raise HTTPException(
                status_code=http_status_error.response.status_code, detail=detail
            ) from http_status_error

        except httpx.ConnectError as exc:
            raise HTTPException(
                status_code=504,
                detail="Service connection error.",
            ) from exc

        except httpx.ConnectTimeout as exc:
            raise HTTPException(
                status_code=504,
                detail="Service connection timeout error.",
            ) from exc

        except Exception as exc:
            raise HTTPException(
                status_code=500,
                detail="OpenAI API call failed",
            ) from exc
