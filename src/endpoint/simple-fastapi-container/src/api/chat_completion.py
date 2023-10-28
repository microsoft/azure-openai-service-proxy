""" OpenAI Async Manager"""


from typing import Tuple
import openai
import openai.error
import openai.openai_object
from fastapi import FastAPI

from tenacity import RetryError

from .openai_async import OpenAIAsyncManager, PlaygroundRequest
from .configuration import OpenAIConfig
from .chat import BaseChat


class ChatCompletion(BaseChat):
    """OpenAI Manager"""

    def __init__(self, app: FastAPI, openai_config: OpenAIConfig):
        """init in memory session manager"""
        super().__init__(app, openai_config)

    def report_exception(
        self, message: str, http_status_code: int
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """report exception"""

        super.logger.warning(msg=f"{message}")

        return message, http_status_code

    async def call_openai_chat_completion(
        self, chat: PlaygroundRequest
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """call openai with retry"""

        completion, http_status_code = self.validate_input(chat)

        if completion or http_status_code:
            return completion, http_status_code

        try:
            async_mgr = OpenAIAsyncManager(self.app, self.openai_config)
            response, deployment_name = await async_mgr.get_openai_chat_completion(chat)

            response["model"] = deployment_name

            return response, 200

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
            super().logger.warning(msg=f"Global exception caught: {exception}")
            raise exception
