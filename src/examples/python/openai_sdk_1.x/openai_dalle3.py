""" dalle-3 example """
import os
import json
from dotenv import load_dotenv

from openai import OpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")

client = OpenAI(
    base_url=ENDPOINT_URL,
    api_key=API_KEY,
)

print("Generating images...")

result = client.images.generate(
    model="dalle3",  # the name of your DALL-E 3 deployment
    prompt="a close-up of a bear walking through the forest",
    n=1,
)

json_response = json.loads(result.model_dump_json())
print(json.dumps(json_response, indent=2))
