""" Completions API """

import logging
from typing import Tuple, Any

from pydantic import BaseModel
import openai
import openai.error
import openai.openai_object
from tenacity import RetryError

from .config import OpenAIConfig
from .openai_async import OpenAIAsyncManager

OPENAI_COMPLETIONS_API_VERSION = "2023-09-01-preview"

logging.basicConfig(level=logging.WARNING)


class CompletionsRequest(BaseModel):
    """OpenAI Compeletion Request"""

    prompt: str | list[str]
    max_tokens: int = None
    temperature: float = None
    top_p: float | None = None
    stop: Any | None = None
    frequency_penalty: float = 0
    presence_penalty: float = 0
    api_version: str = OPENAI_COMPLETIONS_API_VERSION


class Completions:
    """OpenAI Completions Manager"""

    def __init__(self, openai_config: OpenAIConfig):
        """init in memory session manager"""
        self.openai_config = openai_config
        self.logger = logging.getLogger(__name__)

    def __validate_input(self, cr: CompletionsRequest):
        """validate input"""
        # do some basic input validation
        if not cr.prompt:
            return self.__report_exception("Oops, no prompt.", 400)

        # check the max_tokens is between 1 and 4000
        if cr.max_tokens and not 1 <= cr.max_tokens <= 4000:
            return self.__report_exception(
                "Oops, max_tokens must be between 1 and 4000.", 400
            )

        # check the temperature is between 0 and 1
        if cr.temperature and not 0 <= cr.temperature <= 1:
            return self.__report_exception(
                "Oops, temperature must be between 0 and 1.", 400
            )

        # check the top_p is between 0 and 1
        if cr.top_p and not 0 <= cr.top_p <= 1:
            return self.__report_exception("Oops, top_p must be between 0 and 1.", 400)

        # check the frequency_penalty is between 0 and 1
        if cr.frequency_penalty and not 0 <= cr.frequency_penalty <= 1:
            return self.__report_exception(
                "Oops, frequency_penalty must be between 0 and 1.", 400
            )

        # check the presence_penalty is between 0 and 1
        if cr.presence_penalty and not 0 <= cr.presence_penalty <= 1:
            return self.__report_exception(
                "Oops, presence_penalty must be between 0 and 1.", 400
            )

        # check stop sequence are printable characters
        if cr.stop and not cr.stop.isprintable():
            return self.__report_exception(
                "Oops, stop_sequence must be printable characters.", 400
            )

        return None, None

    def __report_exception(
        self, message: str, http_status_code: int
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """report exception"""

        self.logger.warning(msg=f"{message}")

        return message, http_status_code

    async def call_openai_completion(
        self, cr: CompletionsRequest
    ) -> Tuple[openai.openai_object.OpenAIObject, int]:
        """call openai with retry"""

        completion, http_status_code = self.__validate_input(cr)

        if completion or http_status_code:
            return completion, http_status_code

        try:
            deployment = await self.openai_config.get_deployment()

            openai_request = {
                "prompt": cr.prompt,
                "max_tokens": cr.max_tokens,
                "temperature": cr.temperature,
                "top_p": cr.top_p,
                "stop": cr.stop,
                "frequency_penalty": cr.frequency_penalty,
                "presence_penalty": cr.presence_penalty,
            }

            url = (
                f"https://{deployment.resource_name}.openai.azure.com/openai/deployments/"
                f"{deployment.deployment_name}/completions"
                f"?api-version={cr.api_version}"
            )

            async_mgr = OpenAIAsyncManager(deployment)
            response = await async_mgr.async_openai_post(openai_request, url)

            response["model"] = deployment.friendly_name

            return response, 200

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
            self.logger.warning(msg=f"Global exception caught: {exception}")
            raise exception
