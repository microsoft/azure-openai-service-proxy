""" OpenAI Playground chat completion """

from typing import Tuple
import openai
import openai.error
import openai.openai_object
from fastapi import FastAPI

from .config import OpenAIConfig
from .chat_completions import ChatCompletions, PlaygroundRequest, PlaygroundResponse


class Playground(ChatCompletions):
    """OpenAI Manager"""

    def __init__(self, app: FastAPI, openai_config: OpenAIConfig):
        """init in memory session manager"""
        super().__init__(app, openai_config)

    async def call_chat_playground(
        self, chat: PlaygroundRequest
    ) -> Tuple[PlaygroundResponse, int]:
        """chat playground"""

        completion = PlaygroundResponse.empty()

        response, deployment_name = await self.call_openai_chat_completion(chat)

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
