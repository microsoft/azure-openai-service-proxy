""" Test langchain with openai """

# See documentation at https://gloveboxes.github.io/azure-openai-service-proxy/category/developer-endpoints/

import os

import openai
from dotenv import load_dotenv
from langchain.llms import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
API_VERSION = "2023-09-01-preview"
DEPLOYMENT_NAME = "davinci-002"

openai.api_key = API_KEY
openai.api_base = ENDPOINT_URL

llm = AzureOpenAI(
    deployment_name=DEPLOYMENT_NAME,
    openai_api_version=API_VERSION,
    openai_api_key=API_KEY,
)

print(llm("Tell me a joke"))
