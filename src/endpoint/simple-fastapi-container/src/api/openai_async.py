""" OpenAI Async Manager"""

import json
import logging
from typing import Tuple, AsyncGenerator
from fastapi import HTTPException

import httpx
import openai
import openai.error
import openai.openai_object


from .configuration import Deployment

HTTPX_TIMEOUT_SECONDS = 60
HTTPX_STREAMING_TIMEOUT_SECONDS = 10

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

    async def async_openai_post(
        self, openai_request: str, url: str
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """async openai post"""

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

            return (json.loads(response.text), response.status_code)

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

    async def async_post_streaming(
        self,
        openai_request: str,
        url: str,
    ) -> AsyncGenerator:
        """async rest post"""

        headers = {
            "Content-Type": "application/json",
            "api-key": self.deployment.endpoint_key,
        }

        async def streaming_post_request():
            async with httpx.AsyncClient() as client:
                try:
                    async with client.stream(
                        "POST",
                        url=url,
                        headers=headers,
                        json=openai_request,
                        timeout=HTTPX_STREAMING_TIMEOUT_SECONDS,
                    ) as response:
                        response.raise_for_status()
                        async for chunk in response.aiter_bytes():
                            yield (chunk, response.status_code)

                except httpx.HTTPStatusError as http_status_error:
                    raise HTTPException(
                        status_code=http_status_error.response.status_code,
                        detail="Async Streaming Error",
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

        return streaming_post_request()
