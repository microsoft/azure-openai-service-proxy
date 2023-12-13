""" dalle-3 example """
import os

import openai  # for handling error types
from dotenv import load_dotenv

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")

openai.api_base = ENDPOINT_URL
openai.api_key = API_KEY
openai.api_version = "2023-06-01-preview"
openai.api_type = "azure"

generation_response = openai.Image.create(
    prompt="A painting of a dog", size="1024x1024", n=2  # Enter your prompt text here
)

print(generation_response)
