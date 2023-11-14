''' Test langchain with azure openai '''

import os
from dotenv import load_dotenv
import openai
from langchain.llms import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
API_VERSION = "2023-09-01-preview"
DEPLOYMENT_NAME = "davinci-002"

openai.api_type = "azure"
openai.api_key = API_KEY
openai.api_base = ENDPOINT_URL
openai.api_version = API_VERSION


response = openai.Completion.create(
    engine="text-davinci-002-prod", prompt="This is a test", max_tokens=5
)

print(response)

llm = AzureOpenAI(
    deployment_name=DEPLOYMENT_NAME,
    openai_api_version=API_VERSION,
    openai_api_key=API_KEY,
)

print(llm("Tell me a joke"))
