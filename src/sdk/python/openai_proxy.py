import httpx
import openai.error


api_key = None
api_base = "https://openai-proxy-23uljr-ca.salmonsea-82a61dba.swedencentral.azurecontainerapps.io"


class ChatCompletion:
    @classmethod
    def create(cls, request):
        """Create a chat completion"""
        global api_key, api_base

        url = api_base + "/api/v1/chat/completions"
        headers = {"openai-event-code": api_key}

        response = httpx.post(url=url, headers=headers, json=request, timeout=30)

        if response.status_code == 200:
            return response.json()

        if response.status_code in [400, 404, 415]:
            raise openai.error.InvalidRequestError(
                message=response.text,
                http_status=response.status_code,
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
