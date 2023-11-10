""" OpenAI Async Manager"""


from typing import Tuple
import openai
import openai.error
import openai.openai_object
from fastapi import FastAPI
from pydantic import BaseModel

from tenacity import RetryError

from .openai_async import OpenAIAsyncManager, PlaygroundRequest
from .configuration import OpenAIConfig
from .chat import BaseChat


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


class Playground(BaseChat):
    """OpenAI Manager"""

    def __init__(self, app: FastAPI, openai_config: OpenAIConfig):
        """init in memory session manager"""
        super().__init__(app, openai_config)

    def report_exception(
        self, message: str, http_status_code: int
    ) -> Tuple[PlaygroundResponse, int]:
        """report exception"""

        completion = PlaygroundResponse.empty()
        completion.assistant = {"role": "assistant", "content": message}

        self.logger.warning(msg=f"{message}")

        return completion, http_status_code

    async def call_openai_chat(
        self, chat: PlaygroundRequest
    ) -> Tuple[PlaygroundResponse, int]:
        """call openai with retry"""

        completion, http_status_code = self.validate_input(chat)
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
                    completion.finish_reason = choice.finish_reason

                    if "function_call" in choice.message:
                        completion.assistant = {
                            "role": "assistant",
                            "content": choice.message.function_call,
                        }

                    else:
                        completion.assistant = {
                            "role": "assistant",
                            "content": choice.message.content,
                        }

                        filters = choice.content_filter_results

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
            return self.report_exception(
                str(invalid_request_exception.user_message),
                invalid_request_exception.http_status,
            )

        except openai.error.RateLimitError as rate_limit_exception:
            return self.report_exception(
                "Oops, OpenAI rate limited. Please try again.",
                rate_limit_exception.http_status,
            )

        except openai.error.ServiceUnavailableError as service_unavailable_exception:
            return self.report_exception(
                "Oops, OpenAI unavailable. Please try again.",
                service_unavailable_exception.http_status,
            )

        except openai.error.Timeout as timeout_exception:
            return self.report_exception(
                "Oops, OpenAI timeout. Please try again.",
                timeout_exception.http_status,
            )

        except openai.error.OpenAIError as openai_error_exception:
            return self.report_exception(
                str(openai_error_exception.user_message),
                openai_error_exception.http_status,
            )

        except RetryError:
            return self.report_exception(
                str("OpenAI API retry limit reached..."),
                429,
            )

        except Exception as exception:
            self.logger.warning(msg=f"Global exception caught: {exception}")
            raise exception
