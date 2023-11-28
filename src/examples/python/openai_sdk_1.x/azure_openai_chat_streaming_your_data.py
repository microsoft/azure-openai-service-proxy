""" Test Azure OpenAI Chat Completions Stream API """

# Create a new Azure Cognitive Search index and load an index with Azure content
# https://microsoftlearning.github.io/mslearn-knowledge-mining/Instructions/Labs/10-vector-search-exercise.html

import os
import time
from dotenv import load_dotenv
from openai import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
AZURE_AI_SEARCH_ENDPOINT = os.environ.get("AZURE_AI_SEARCH_ENDPOINT")
AZURE_AI_SEARCH_KEY = os.environ.get("AZURE_AI_SEARCH_KEY")
AZURE_AI_SEARCH_INDEX_NAME = os.environ.get("AZURE_AI_SEARCH_INDEX_NAME")

API_VERSION = "2023-09-01-preview"
MODEL_NAME = "gpt-35-turbo"


client = AzureOpenAI(
    base_url=ENDPOINT_URL,
    api_key=API_KEY,
    api_version=API_VERSION,
)

messages = [
    {
        "role": "user",
        "content": (
            "What are the differences between Azure Machine Learning "
            "and Azure AI services?"
        ),
    },
]

body = {
    "dataSources": [
        {
            "type": "AzureCognitiveSearch",
            "parameters": {
                "endpoint": AZURE_AI_SEARCH_ENDPOINT,
                "key": AZURE_AI_SEARCH_KEY,
                "indexName": AZURE_AI_SEARCH_INDEX_NAME,
            },
        }
    ]
}


response = client.chat.completions.create(
    model="gpt-3.5-turbo",
    messages=messages,
    extra_body=body,
    stream=True,
    max_tokens=100,
)


for chunk in response:
    if chunk.choices and len(chunk.choices) > 0:
        content = chunk.choices[0].delta.content
        if content:
            print(content, end="")
        # delay to simulate real-time chat
        time.sleep(0.05)

print()
