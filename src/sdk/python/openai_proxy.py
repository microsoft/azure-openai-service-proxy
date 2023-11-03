import httpx
import openai.error
from typing import Any


api_key = None
api_base = "https://openai-proxy-23uljr-ca.salmonsea-82a61dba.swedencentral.azurecontainerapps.io"


class ChatCompletion:
    @classmethod
    def create(cls,
        messages : str,
        max_tokens : int,
        temperature : float,
        model: str ="gpt-3.5-turbo-0613",
        top_p : float | None = None,
        stop_sequence: str | None = None,
        frequency_penalty : float | None = None,
        presence_penalty: float | None = None,
        functions: list[dict[str, Any]] | None = None,
        function_call: str | dict[str, str] = "auto"):

        # note, model, not used but here for OpenAI API compatibility

        request = {
            "messages": messages,
            "max_tokens": max_tokens,
            "temperature": temperature,
        }

        if functions:
            request["functions"] = functions
            request["function_call"] = function_call

        if top_p:
            request["top_p"] = top_p

        if stop_sequence:
            request["stop_sequence"] = stop_sequence

        if frequency_penalty:
            request["frequency_penalty"] = frequency_penalty

        if presence_penalty:
            request["presence_penalty"] = presence_penalty


        """Create a chat completion"""
        global api_key, api_base

        url = api_base + "/api/v1/chat/completions"
        headers = {"openai-event-code": api_key}

        response = httpx.post(url=url, headers=headers, json=request, timeout=30)

        if response.status_code == 200:
            return response.json()

        elif response.status_code in [400, 404, 415]:
            raise openai.error.InvalidRequestError(
                message=response.text,
                http_status=response.status_code,
                param="",
            )

        elif response.status_code == 401:
            raise openai.error.AuthenticationError(
                message=response.text,
                http_status=response.status_code,
            )

        elif response.status_code == 403:
            raise openai.error.PermissionError(
                message=response.text,
                http_status=response.status_code,
            )

        elif response.status_code == 409:
            raise openai.error.TryAgain(
                message=response.text,
                http_status=response.status_code,
            )

        elif response.status_code == 429:
            raise openai.error.RateLimitError(
                message=response.text,
                http_status=response.status_code,
            )
        else:
            raise openai.error.APIError(
                message=response.text,
                http_status=response.status_code,
            )
