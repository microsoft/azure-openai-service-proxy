"""Model classes for the API."""
from enum import Enum


# create a model type enum
class DeploymentClass(Enum):
    """Deployment Class."""

    OPENAI_CHAT = "openai-chat"
    OPENAI_CHAT_EXTENSIONS = "openai-chat-extensions"
    OPENAI_COMPLETIONS = "openai-completions"
    OPENAI_EMBEDDINGS = "openai-embeddings"
    OPENAI_IMAGES_GENERATIONS = "openai-images-generations"
    OPENAI_IMAGES = "openai-images"
    EVENT_INFO = "*"
