""" Test Azure OpenAI Embeddings API """

import os
from dotenv import load_dotenv
import openai

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
API_VERSION = "2023-08-01-preview"
DEPLOYMENT_NAME = "text-embedding-ada-002"

openai.api_type = "azure"
openai.api_key = API_KEY
openai.api_base = ENDPOINT_URL


content = (
    "This stunning leather wrap bracelet will add a touch of bohemian flair to your outfit."
    "The bracelet features a braided leather band in a rich brown color, adorned with turquoise beads and silver charms. "
    "The bracelet wraps around your wrist multiple times, creating a layered look that is eye-catching and stylish. "
    "The bracelet is adjustable and has a button closure for a secure fit. "
    "This leather wrap bracelet is the perfect accessory for any occasion, "
    "whether you want to dress up a casual outfit or add some color to a formal one."
)


query_embeddings = openai.Embedding.create(
    engine=DEPLOYMENT_NAME,
    input=str(content),
    encoding_format="float",
    api_version="2023-08-01-preview",
)

print(query_embeddings)
print(query_embeddings.data[0].embedding)
