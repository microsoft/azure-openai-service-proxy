import logging


from fastapi import FastAPI
from .openai_async_chat import PlaygroundRequest
from .configuration import OpenAIConfig

logging.basicConfig(level=logging.WARNING)


class BaseChat:
    def __init__(self, app: FastAPI, openai_config: OpenAIConfig):
        """init in memory session manager"""
        self.openai_config = openai_config
        self.app = app
        self.logger = logging.getLogger(__name__)

    def validate_input(self, chat: PlaygroundRequest):
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
