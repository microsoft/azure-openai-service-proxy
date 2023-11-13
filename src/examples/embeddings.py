from openai import OpenAI

client = OpenAI(
    base_url="YOUR_PROXY_API_URL",
    api_key="YOUR_EVENT_CODE/GITHUB_USERNAME",
)

print("\nCalling Embeddings API\n")

response = client.embeddings.create(
    input="Your text string goes here", model="text-embedding-ada-002"
)

print(response.data[0].embedding)