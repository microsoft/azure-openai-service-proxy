""" Test completions with azure openai """

# See documentation at https://gloveboxes.github.io/azure-openai-service-proxy/category/developer-endpoints/

import os

from dotenv import load_dotenv
from openai import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
API_VERSION = "2023-09-01-preview"
MODEL_NAME = "davinci-002"


client = AzureOpenAI(
    azure_endpoint=ENDPOINT_URL,
    api_key=API_KEY,
    api_version=API_VERSION,
)

response = client.completions.create(model=MODEL_NAME, prompt="write a 50 word poem about elephants", max_tokens=512)

print(response)
