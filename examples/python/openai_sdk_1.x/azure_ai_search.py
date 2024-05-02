# For info on loading an Azure AI Search index with Azure content, see:
# https://learn.microsoft.com/en-us/training/modules/improve-search-results-vector-search/?WT.mc_id=academic-107233-lbugnion

import json
import os

from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient

service_endpoint = os.environ["AZURE_SEARCH_SERVICE_ENDPOINT"]
index_name = os.environ["AZURE_SEARCH_INDEX_NAME"]
key = os.environ["AZURE_SEARCH_API_KEY"]

search_client = SearchClient(service_endpoint, index_name, AzureKeyCredential(key))

results = search_client.search(
    search_text="What are the differences between Azure Machine Learning and Azure AI services?",
    top=4,
)

for result in results:
    # delete the contentVector field from the result as it is too large to display
    if "contentVector" in result:
        del result["contentVector"]
    # result is json object pretty print the result
    print(json.dumps(result, indent=4))
