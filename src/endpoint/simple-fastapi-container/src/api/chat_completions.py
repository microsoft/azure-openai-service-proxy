import logging
from fastapi import FastAPI
from pydantic import BaseModel
from .config import OpenAIConfig

from typing import Tuple, Any
import openai
import openai.error
import openai.openai_object
from tenacity import RetryError

from .openai_async import OpenAIAsyncManager

logging.basicConfig(level=logging.WARNING)


class ChatCompletionsRequest(BaseModel):
    """OpenAI Chat Request"""

    messages: list[dict[str, str]]
    max_tokens: int
    temperature: float
    top_p: float | None = None
    stop_sequence: Any | None = None
    frequency_penalty: float | None = None
    presence_penalty: float | None = None
    functions: list[dict[str, Any]] | None = None
    function_call: str | dict[str, str] = "auto"


class ChatCompletions:
    def __init__(self, app: FastAPI, openai_config: OpenAIConfig):
        """init in memory session manager"""
        self.openai_config = openai_config
        self.app = app
        self.logger = logging.getLogger(__name__)

    def validate_input(self, chat: ChatCompletionsRequest):
        """validate input"""
        # do some basic input validation
        if not chat.messages:
            return self.report_exception("Oops, no chat messages.", 400)

        # check the max_tokens is between 1 and 4000
        if not 1 <= chat.max_tokens <= 4000:
            return self.report_exception(
                "Oops, max_tokens must be between 1 and 4000.", 400
            )

        # check the temperature is between 0 and 1
        if not 0 <= chat.temperature <= 1:
            return self.report_exception(
                "Oops, temperature must be between 0 and 1.", 400
            )

        # check the top_p is between 0 and 1
        if chat.top_p and not 0 <= chat.top_p <= 1:
            return self.report_exception("Oops, top_p must be between 0 and 1.", 400)

        # check the frequency_penalty is between 0 and 1
        if chat.frequency_penalty and not 0 <= chat.frequency_penalty <= 1:
            return self.report_exception(
                "Oops, frequency_penalty must be between 0 and 1.", 400
            )

        # check the presence_penalty is between 0 and 1
        if chat.presence_penalty and not 0 <= chat.presence_penalty <= 1:
            return self.report_exception(
                "Oops, presence_penalty must be between 0 and 1.", 400
            )

        # check stop sequence are printable characters
        if chat.stop_sequence and not chat.stop_sequence.isprintable():
            return self.report_exception(
                "Oops, stop_sequence must be printable characters.", 400
            )

        return None, None

    def report_exception(
        self, message: str, http_status_code: int
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """report exception"""

        self.logger.warning(msg=f"{message}")

        return message, http_status_code

    async def call_openai_chat_completion(
        self, chat: ChatCompletionsRequest
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """call openai with retry"""

        completion, http_status_code = self.validate_input(chat)

        if completion or http_status_code:
            return completion, http_status_code

        try:
            deployment = await self.openai_config.get_deployment()

            if chat.functions:
                # https://platform.openai.com/docs/guides/gpt/function-calling
                # function_call cabn = none, auto, or {"name": "<insert-function-name>"}
                openai_request = {
                    "messages": chat.messages,
                    "max_tokens": chat.max_tokens,
                    "temperature": chat.temperature,
                    "functions": chat.functions,
                    "function_call": chat.function_call
                    if chat.function_call
                    else "auto",
                }

            else:
                openai_request = {
                    "messages": chat.messages,
                    "max_tokens": chat.max_tokens,
                    "temperature": chat.temperature,
                }

            if chat.top_p is not None:
                openai_request["top_p"] = chat.top_p

            if chat.stop_sequence is not None:
                openai_request["stop"] = chat.stop_sequence

            if chat.frequency_penalty is not None:
                openai_request["frequency_penalty"] = chat.frequency_penalty

            if chat.presence_penalty is not None:
                openai_request["presence_penalty"] = chat.presence_penalty

            url = (
                f"https://{deployment.resource_name}.openai.azure.com/openai/deployments/"
                f"{deployment.deployment_name}/chat/completions?api-version={deployment.api_version}"
            )

            async_mgr = OpenAIAsyncManager(self.app, deployment)
            response, deployment_name = await async_mgr.call_openai(openai_request, url)

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
            self.logger.warning(msg=f"Global exception caught: {exception}")
            raise exception
