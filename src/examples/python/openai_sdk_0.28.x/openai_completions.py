""" Test langchain with openai """ ""

import os
from dotenv import load_dotenv
import openai


load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
API_VERSION = "2023-09-01-preview"
DEPLOYMENT_NAME = "davinci-002"
ENGINE_NAME = "text-davinci-002-prod"

openai.api_key = API_KEY
openai.api_base = ENDPOINT_URL


response = openai.Completion.create(
    engine=ENGINE_NAME, prompt="This is a test", max_tokens=5
)

print(response)
