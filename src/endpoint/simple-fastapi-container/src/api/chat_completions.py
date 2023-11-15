""" Chat Completions API """

import logging
from typing import Tuple, Any
import httpx

from fastapi import FastAPI
from pydantic import BaseModel
import openai
import openai.error
import openai.openai_object

from .config import OpenAIConfig
from .openai_async import OpenAIAsyncManager, OpenAIException

OPENAI_CHAT_COMPLETIONS_API_VERSION = "2023-09-01-preview"

logging.basicConfig(level=logging.WARNING)


class ChatCompletionsRequest(BaseModel):
    """OpenAI Chat Request"""

    messages: list[dict[str, str]]
    max_tokens: int = None
    temperature: float = None
    top_p: float | None = None
    stop_sequence: Any | None = None
    frequency_penalty: float = 0
    presence_penalty: float = 0
    functions: list[dict[str, Any]] | None = None
    function_call: str | dict[str, str] = "auto"
    api_version: str = OPENAI_CHAT_COMPLETIONS_API_VERSION


class ChatCompletions:
    """OpenAI Chat Completions Manager"""

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
        if chat.max_tokens and not 1 <= chat.max_tokens <= 4000:
            return self.report_exception(
                "Oops, max_tokens must be between 1 and 4000.", 400
            )

        # check the temperature is between 0 and 1
        if chat.temperature and not 0 <= chat.temperature <= 1:
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
                # function_call can be = none, auto, or {"name": "<insert-function-name>"}
                openai_request = {
                    "messages": chat.messages,
                    "max_tokens": chat.max_tokens,
                    "temperature": chat.temperature,
                    "functions": chat.functions,
                    "function_call": chat.function_call,
                }

            else:
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
                f"https://{deployment.resource_name}.openai.azure.com/openai/deployments/"
                f"{deployment.deployment_name}/chat/completions"
                f"?api-version={chat.api_version}"
            )

            async_mgr = OpenAIAsyncManager(self.app, deployment)
            response = await async_mgr.async_openai_post(openai_request, url)

            response["model"] = deployment.friendly_name

            return response, 200

        except httpx.ConnectError:
            return self.report_exception(
                "Service connection error.",
                504,
            )

        except httpx.ConnectTimeout:
            return self.report_exception(
                "Service connection timeout error.",
                504,
            )

        except OpenAIException as openai_exception:
            return self.report_exception(
                str(openai_exception),
                openai_exception.http_status_code,
            )

        except Exception as exception:
            self.logger.warning(msg=f"Global exception caught: {exception}")
            raise exception
