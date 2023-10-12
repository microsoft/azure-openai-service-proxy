""" OpenAI Async Manager"""


import logging
from typing import Tuple
import openai
import openai.error
import openai.openai_object
from fastapi import FastAPI
from pydantic import BaseModel

from tenacity import RetryError

from .openai_async import OpenAIAsyncManager, PlaygroundRequest
from .configuration import OpenAIConfig

logging.basicConfig(level=logging.WARNING)
logger = logging.getLogger(__name__)


class PlaygroundResponse(BaseModel):
    """Playground Chat Completion Response"""

    assistant: dict[str, str]
    finish_reason: str
    response_ms: int
    content_filtered: dict[str, dict[str, str | bool]]
    usage: dict[str, dict[str, int]]
    name: str

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
            name="",
        )


class Playground:
    """OpenAI Manager"""

    def __init__(self, app: FastAPI, openai_config: OpenAIConfig):
        """init in memory session manager"""
        self.openai_config = openai_config
        self.app = app

    def __report_exception(
        self, message: str, http_status_code: int
    ) -> Tuple[PlaygroundResponse, int]:
        """report exception"""

        completion = PlaygroundResponse.empty()
        completion.assistant = {"role": "assistant", "content": message}

        logger.warning(msg=f"{message}")

        return completion, http_status_code

    def __validate_input(self, chat: PlaygroundRequest):
        """validate input"""
        # do some basic input validation
        if not chat.messages:
            return self.__report_exception("Oops, no chat messages.", 400)

        # search through the list of messages for a msg with a role of user
        user_message = next(
            (msg for msg in chat.messages if msg["role"] == "user"), None
        )

        if not user_message or user_message.get("content", "") == "":
            return self.__report_exception(
                "Oops, messages missing a user role message.", 400
            )

        # check the max_tokens is between 1 and 4096
        if not 1 <= chat.max_tokens <= 4000:
            return self.__report_exception(
                "Oops, max_tokens must be between 1 and 4000.", 400
            )

        # check the temperature is between 0 and 1
        if not 0 <= chat.temperature <= 1:
            return self.__report_exception(
                "Oops, temperature must be between 0 and 1.", 400
            )

        # check the top_p is between 0 and 1
        if not 0 <= chat.top_p <= 1:
            return self.__report_exception("Oops, top_p must be between 0 and 1.", 400)

        # check the frequency_penalty is between 0 and 1
        if not 0 <= chat.frequency_penalty <= 1:
            return self.__report_exception(
                "Oops, frequency_penalty must be between 0 and 1.", 400
            )

        # check the presence_penalty is between 0 and 1
        if not 0 <= chat.presence_penalty <= 1:
            return self.__report_exception(
                "Oops, presence_penalty must be between 0 and 1.", 400
            )

        # check stop sequence are printable characters
        if not chat.stop_sequence.isprintable():
            return self.__report_exception(
                "Oops, stop_sequence must be printable characters.", 400
            )

        return None, None

    async def call_openai_chat(self, chat: PlaygroundRequest) -> Tuple[PlaygroundResponse, int]:
        """call openai with retry"""

        completion, http_status_code = self.__validate_input(chat)
        if completion or http_status_code:
            return completion, http_status_code

        completion = PlaygroundResponse.empty()

        try:
            async_mgr = OpenAIAsyncManager(self.app, self.openai_config)
            response, deployment_name = await async_mgr.get_openai_chat_completion(chat)

            if response and isinstance(response, openai.openai_object.OpenAIObject):
                completion.response_ms = response.response_ms
                completion.name = deployment_name

                completion.usage = {
                    "usage": {
                        "completion_tokens": response.usage.completion_tokens,
                        "prompt_tokens": response.usage.prompt_tokens,
                        "total_tokens": response.usage.total_tokens,
                        "max_tokens": chat.max_tokens,
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
            return self.__report_exception(
                str(invalid_request_exception.user_message),
                invalid_request_exception.http_status,
            )

        except openai.error.RateLimitError as rate_limit_exception:
            return self.__report_exception(
                "Oops, OpenAI rate limited. Please try again.",
                rate_limit_exception.http_status,
            )

        except openai.error.ServiceUnavailableError as service_unavailable_exception:
            return self.__report_exception(
                "Oops, OpenAI unavailable. Please try again.",
                service_unavailable_exception.http_status,
            )

        except openai.error.Timeout as timeout_exception:
            return self.__report_exception(
                "Oops, OpenAI timeout. Please try again.",
                timeout_exception.http_status,
            )

        except openai.error.OpenAIError as openai_error_exception:
            return self.__report_exception(
                str(openai_error_exception.user_message),
                openai_error_exception.http_status,
            )

        except RetryError:
            return self.__report_exception(
                str("OpenAI API retry limit reached..."),
                429,
            )

        except Exception as exception:
            logger.warning(msg=f"Global exception caught: {exception}")
            raise exception
