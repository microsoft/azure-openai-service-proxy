""" OpenAI Async Manager"""


from typing import Tuple
import openai
import openai.error
import openai.openai_object
from fastapi import FastAPI

from tenacity import RetryError

from .openai_async import OpenAIAsyncManager

from .config import OpenAIConfig
from .chat import BaseChat, PlaygroundRequest


class ChatCompletions(BaseChat):
    """OpenAI Manager"""

    def __init__(self, app: FastAPI, openai_config: OpenAIConfig):
        """init in memory session manager"""
        super().__init__(app, openai_config)

    def report_exception(
        self, message: str, http_status_code: int
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """report exception"""

        self.logger.warning(msg=f"{message}")

        return message, http_status_code

    async def call_openai_chat_completion(
        self, chat: PlaygroundRequest
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

            # url = (
            #     f"https://{deployment.endpoint_location}.api.cognitive.microsoft.com/openai/deployments/"
            #     f"{deployment.deployment_name}/chat/completions?api-version={deployment.api_version}"
            # )

            url = (
                f"https://{deployment.resource_name}.openai.azure.com/openai/deployments/"
                f"{deployment.deployment_name}/chat/completions?api-version={deployment.api_version}"
            )

            async_mgr = OpenAIAsyncManager(self.app, deployment)
            response, deployment_name = await async_mgr.call_openai(openai_request, url)

            # async_mgr = OpenAIAsyncManager(self.app, self.openai_config)
            # response, deployment_name = await async_mgr.get_openai_chat_completion(chat)

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
