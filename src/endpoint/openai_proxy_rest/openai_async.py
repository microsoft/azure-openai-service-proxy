""" OpenAI Async Manager"""

import logging
from typing import Any
from pydantic import BaseModel
import openai

from tenacity import (
    retry,
    stop_after_attempt,
    wait_random_exponential,
    retry_if_not_exception_type,
)


class OpenAIChat(BaseModel):
    """vector query"""

    prompt: str
    user: list[str]
    system: list[str]
    assistant: list[str]
    max_tokens: int = 1024
    temperature: float
    top_p: float
    stop_sequence: str
    frequency_penalty: float
    presence_penalty: float


class OpenAIChatCompletion(BaseModel):
    """list of video results"""

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
        openai_key: str,
        openai_endpoint: str,
        openai_version: str,
        model_deployment_name: str,
        gpt_model_name: str,
    ):
        """init in memory session manager"""
        self.openai_key = openai_key
        self.openai_endpoint = openai_endpoint
        self.openai_version = openai_version
        self.model_deployment_name = model_deployment_name
        self.gpt_model_name = gpt_model_name


class OpenAIAsyncManager:
    """OpenAI Manager"""

    def __init__(self, openai_config: OpenAIConfig):
        """init in memory session manager"""
        self.openai_config = openai_config

    async def async_get_chat_completion(self, chat: OpenAIChat) -> Any:
        """Gets the OpenAI functions from the text."""

        msgs = list(map(lambda x: {"role": "system", "content": x}, chat.system))
        msgs += list(map(lambda x: {"role": "user", "content": x}, chat.user))
        msgs += list(map(lambda x: {"role": "assistant", "content": x}, chat.assistant))

        response = await openai.ChatCompletion.acreate(
            messages=msgs,
            temperature=chat.temperature,
            max_tokens=chat.max_tokens,
            top_p=chat.top_p,
            stop=chat.stop_sequence,
            frequency_penalty=chat.frequency_penalty,
            presence_penalty=chat.presence_penalty,
            engine=self.openai_config.model_deployment_name,
            request_timeout=30,
        )

        return response


class OpenAIManager:
    """OpenAI Manager"""

    def __init__(self, openai_config: OpenAIConfig):
        """init in memory session manager"""
        self.openai_config = openai_config

        openai.api_type = "azure"
        openai.api_key = openai_config.openai_key
        openai.api_base = openai_config.openai_endpoint
        openai.api_version = openai_config.openai_version

        logging.basicConfig(level=logging.WARNING)
        self.logger = logging.getLogger(__name__)

    def _report_exception(self, message: str) -> OpenAIChatCompletion:
        """report exception"""

        completion = OpenAIChatCompletion.empty()
        completion.assistant = {"role": "assistant", "content": message}

        self.logger.warning(msg=f"{message}")

        return completion

    @retry(
        wait=wait_random_exponential(min=4, max=12),
        stop=stop_after_attempt(2),
        retry=retry_if_not_exception_type(openai.InvalidRequestError),
    )
    async def call_openai_chat(self, chat: OpenAIChat) -> OpenAIChatCompletion:
        """call openai with retry"""

        completion = OpenAIChatCompletion.empty()

        try:
            async_mgr = OpenAIAsyncManager(self.openai_config)
            response = await async_mgr.async_get_chat_completion(chat)

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

            return completion

        except openai.error.InvalidRequestError as invalid_request_exception:
            # this exception captures content policy violation policy
            return self._report_exception(str(invalid_request_exception.user_message))

        except openai.error.RateLimitError:
            return self._report_exception(
                "Oops, OpenAI rate limited. Please try again."
            )

        except openai.error.ServiceUnavailableError:
            return self._report_exception("Oops, OpenAI unavailable. Please try again.")

        except openai.error.Timeout:
            return self._report_exception("Oops, OpenAI timeout. Please try again.")

        except openai.error.OpenAIError as openai_error_exception:
            return self._report_exception(str(openai_error_exception.user_message))

        except Exception as exception:
            self.logger.warning(msg=f"Global exception caught: {exception}")
            raise exception
