""" OpenAI Async Manager"""

import json
import time
import logging
from typing import Any
import openai
import openai.error
import openai.openai_object
from pydantic import BaseModel
import httpx
from fastapi import FastAPI

from tenacity import (
    retry,
    stop_after_attempt,
    wait_random_exponential,
    retry_if_not_exception_type,
)


class OpenAIChatRequest(BaseModel):
    """OpenAI Chat Request"""

    messages: list[dict[str, str]]
    max_tokens: int = 1024
    temperature: float
    top_p: float
    stop_sequence: str
    frequency_penalty: float
    presence_penalty: float


class OpenAIChatResponse(BaseModel):
    """Chat Completion Response"""

    assistant: dict[str, str]
    finish_reason: str
    response_ms: int
    content_filtered: dict[str, dict[str, str | bool]]
    usage: dict[str, dict[str, int]]

    # create an empty response
    @classmethod
    def empty(cls):
        """empty response"""
        return cls(
            assistant={},
            finish_reason="",
            response_ms=0,
            content_filtered={},
            usage={},
        )


class OpenAIConfig:
    """OpenAI Parameters"""

    def __init__(
        self,
        *,
        openai_version: str,
        gpt_model_name: str,
        deployments: list[dict[str, str]],
        request_timeout: int,
    ):
        """init in memory session manager"""
        self.openai_version = openai_version
        self.gpt_model_name = gpt_model_name
        self.deployments = deployments
        self.request_timeout = request_timeout


class OpenAIAsyncManager:
    """OpenAI Manager"""

    def __init__(self, app: FastAPI, openai_config: OpenAIConfig):
        """init in memory session manager"""
        self.openai_config = openai_config
        self.app = app

    async def get_openai_chat_completion(
        self, chat: OpenAIChatRequest
    ) -> openai.openai_object.OpenAIObject:
        """async get openai completion"""

        def get_error(response: httpx.Response) -> dict[str, dict[str, Any]] | None:
            """get error message from response"""
            try:
                return json.loads(response.text).get("error")
            except json.JSONDecodeError:
                return None

        index = int(self.app.state.round_robin % len(self.openai_config.deployments))
        self.app.state.round_robin += 1

        api_version = self.openai_config.openai_version
        deployment_name = self.openai_config.deployments[index]["deployment_name"]
        api_key = self.openai_config.deployments[index]["key"]
        resource_name = self.openai_config.deployments[index]["endpoint"]

        openai_request = {
            "messages": chat.messages,
            "max_tokens": chat.max_tokens,
            "temperature": chat.temperature,
            "top_p": chat.top_p,
            "stop": chat.stop_sequence,
            "frequency_penalty": chat.frequency_penalty,
            "presence_penalty": chat.presence_penalty,
        }

        url = (
            f"https://{resource_name}.openai.azure.com/openai/deployments/"
            f"{deployment_name}/chat/completions?api-version={api_version}"
        )

        headers = {"Content-Type": "application/json", "api-key": api_key}

        start = time.time()

        try:
            async with httpx.AsyncClient() as client:
                response = await client.post(
                    url, headers=headers, json=openai_request, timeout=30
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

        return openai_response


class OpenAIManager:
    """OpenAI Manager"""

    def __init__(self, app: FastAPI, openai_config: OpenAIConfig):
        """init in memory session manager"""
        self.openai_config = openai_config
        self.app = app

        logging.basicConfig(level=logging.WARNING)
        self.logger = logging.getLogger(__name__)

    def _report_exception(
        self, message: str, http_status_code: int
    ) -> OpenAIChatResponse:
        """report exception"""

        completion = OpenAIChatResponse.empty()
        completion.assistant = {"role": "assistant", "content": message}

        self.logger.warning(msg=f"{message}")

        return completion, http_status_code

    @retry(
        wait=wait_random_exponential(min=4, max=10),
        stop=stop_after_attempt(2),
        retry=retry_if_not_exception_type(openai.InvalidRequestError),
    )
    async def call_openai_chat(self, chat: OpenAIChatRequest) -> OpenAIChatResponse:
        """call openai with retry"""

        completion = OpenAIChatResponse.empty()

        try:
            async_mgr = OpenAIAsyncManager(self.app, self.openai_config)
            response = await async_mgr.get_openai_chat_completion(chat)

            if response and isinstance(response, openai.openai_object.OpenAIObject):
                completion.response_ms = response.response_ms

                completion.usage = {
                    "usage": {
                        "completion_tokens": response.usage.completion_tokens,
                        "prompt_tokens": response.usage.prompt_tokens,
                        "total_tokens": response.usage.total_tokens,
                    }
                }

                choices = response.choices
                if len(response.choices) > 0:
                    choice = choices[0]
                    filters = choice.content_filter_results

                    completion.assistant = {
                        "role": "assistant",
                        "content": choice.message.content,
                    }

                    completion.finish_reason = choice.finish_reason

                    completion.content_filtered = {
                        "hate": {
                            "filtered": filters.hate.filtered,
                            "severity": filters.hate.severity,
                        },
                        "self_harm": {
                            "filtered": filters.self_harm.filtered,
                            "severity": filters.self_harm.severity,
                        },
                        "sexual": {
                            "filtered": filters.sexual.filtered,
                            "severity": filters.sexual.severity,
                        },
                    }

            return completion, 200

        except openai.error.InvalidRequestError as invalid_request_exception:
            # this exception captures content policy violation policy
            return self._report_exception(
                str(invalid_request_exception.user_message),
                invalid_request_exception.http_status,
            )

        except openai.error.RateLimitError as rate_limit_exception:
            return self._report_exception(
                "Oops, OpenAI rate limited. Please try again.",
                rate_limit_exception.http_status,
            )

        except openai.error.ServiceUnavailableError as service_unavailable_exception:
            return self._report_exception(
                "Oops, OpenAI unavailable. Please try again.",
                service_unavailable_exception.http_status,
            )

        except openai.error.Timeout as timeout_exception:
            return self._report_exception(
                "Oops, OpenAI timeout. Please try again.",
                timeout_exception.http_status,
            )

        except openai.error.OpenAIError as openai_error_exception:
            return self._report_exception(
                str(openai_error_exception.user_message),
                openai_error_exception.http_status,
            )

        except Exception as exception:
            self.logger.warning(msg=f"Global exception caught: {exception}")
            raise exception
