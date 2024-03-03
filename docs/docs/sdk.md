# SDK support

The Azure AI Proxy is a transparent proxy service that supports several Azure AI service including Azure OpenAI SDKs, Azure AI Search SDKs, LangChain, and REST endpoints for developer events, workshops, and hackathons. Access is granted using a time bound API Key and the Azure AI Proxy endpoint URL.

## Azure AI Proxy SDK access

For SDK access to Azure AI Proxy services, you need two things:

1. The Azure AI Proxy endpoint URL. Note, the endpoint URL is prefixed with /api/v1.
1. A time bound API Key.

The Azure AI Proxy service URL is provided by the event organizer. The time bound API Key is provided to the attendees when they register for the event.

## Azure OpenAI SDKs

The Azure AI Proxy provides support for the following Azure OpenAI SDKs:

1. Azure OpenAI Chat Completions
1. Azure OpenAI Completion
1. Azure OpenAI Embeddings
1. Azure OpenAI DALL-E 2
1. Azure OpenAI DALL-E 3

### Azure OpenAI Python SDK example

The following is an example of calling the Azure OpenAI Chat Completions API using the Azure OpenAI Python SDK.

```python
""" Test Azure OpenAI Chat Completions API """

ENDPOINT_URL = "https://<YOUR_PROXY_PLAYGROUND_URL>/api/v1"
API_KEY = "36a69054-0956-4eb6-963a-571089d46c58"

import os

from dotenv import load_dotenv
from openai import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")
API_VERSION = "2023-09-01-preview"
MODEL_NAME = "gpt-35-turbo"


client = AzureOpenAI(
    azure_endpoint=ENDPOINT_URL,
    api_key=API_KEY,
    api_version=API_VERSION,
)

MESSAGES = [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Who won the world series in 2020?"},
    {
        "role": "assistant",
        "content": "The Los Angeles Dodgers won the World Series in 2020.",
    },
    {"role": "user", "content": "Where was it played?"},
]


completion = client.chat.completions.create(
    model=MODEL_NAME,
    messages=MESSAGES,
)

print(completion.model_dump_json(indent=2))
print()
print(completion.choices[0].message.content)
```

## Azure AI Search Query

The Azure AI Proxy provides support for Azure AI Search queries using the Azure OpenAI proxy service. Access to these services is granted using a time bound event code. The proxy supports Azure AI Search [POST REST](https://learn.microsoft.com/azure/search/search-get-started-rest#search-an-index) and [POST ODATA](https://learn.microsoft.com/azure/search/query-odata-filter-orderby-syntax) queries.

Create a read-only Query API Key for the Azure AI Search service and use it with the Azure AI Proxy service.

![Azure AI Search](media/ai-search-query-key.png)

### Azure AI Search Python SDK example

The following is an example of calling the Azure AI Search API using the Azure AI Search Python SDK. Note, you can use with Azure Prompt Flow to retrieve the documentation.

```python
""" Test Azure AI Search API """

```python
""" Test Azure AI Search API """

```python
ENDPOINT_URL = "https://<YOUR_PROXY_PLAYGROUND_URL>/api/v1"
API_KEY = "36a69054-0956-4eb6-963a-571089d46c58"

def retrieve_documentation(
    question: str,
    index_name: str,
    embedding: List[float],
    search: CognitiveSearchConnection,
) -> str:

    search_client = SearchClient(
        endpoint=ENDPOINT_URL,
        index_name=index_name,
        credential=AzureKeyCredential(API_KEY),
    )

    vector_query = VectorizedQuery(
        vector=embedding, k_nearest_neighbors=3, fields="contentVector"
    )

    results = search_client.search(
        search_text=question,
        vector_queries=[vector_query],
        query_type=QueryType.SEMANTIC,
        semantic_configuration_name="default",
        query_caption=QueryCaptionType.EXTRACTIVE,
        query_answer=QueryAnswerType.EXTRACTIVE,
        top=3,
    )

    docs = [
        {
            "id": doc["id"],
            "title": doc["title"],
            "content": doc["content"],
            "url": doc["url"],
        }
        for doc in results
    ]

    return docs
```
