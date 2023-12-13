""" Test OpenAI Embeddings API """

# See documentation at https://gloveboxes.github.io/azure-openai-service-proxy/category/developer-endpoints/

import os

import openai
from dotenv import load_dotenv

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")

OPENAI_EMBEDDING_ENGINE = "text-embedding-ada-002"

openai.api_key = API_KEY
openai.api_base = ENDPOINT_URL


content = (
    "This stunning leather wrap bracelet will add a touch of bohemian flair to your outfit."
    "The bracelet features a braided leather band in a rich brown color, adorned with turquoise beads and silver charms. "  # noqa: E501
    "The bracelet wraps around your wrist multiple times, creating a layered look that is eye-catching and stylish. "
    "The bracelet is adjustable and has a button closure for a secure fit. "
    "This leather wrap bracelet is the perfect accessory for any occasion, "
    "whether you want to dress up a casual outfit or add some color to a formal one."
)


query_embeddings = openai.Embedding.create(
    engine=OPENAI_EMBEDDING_ENGINE,
    input=str(content),
    encoding_format="float",
)

print(query_embeddings)
print(query_embeddings.data[0].embedding)
