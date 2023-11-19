''' dalle-3 example '''
import os
from dotenv import load_dotenv
import openai                  # for handling error types
# from openai import OpenAI
from openai import AzureOpenAI

load_dotenv()

ENDPOINT_URL = os.environ.get("ENDPOINT_URL")
API_KEY = os.environ.get("API_KEY")

client = AzureOpenAI(
    base_url=ENDPOINT_URL,
    api_key=API_KEY,
    api_version="2023-12-01-preview",
)

prompt = (
 "Subject: ballet dancers posing on a beam. "  # use the space at end
 "Style: romantic impressionist painting."     # this is implicit line continuation
)

image_params = {
 "model": "dall-e-2",  # Defaults to dall-e-2
 "n": 1,               # Between 2 and 10 is only for DALL-E 2
 "size": "1024x1024",  # 256x256, 512x512 only for DALL-E 2 - not much cheaper
 "prompt": prompt,     # DALL-E 3: max 4000 characters, DALL-E 2: max 1000
 "user": "myName",     # pass a customer ID to OpenAI for abuse monitoring
}

## -- You can uncomment the lines below to include these non-default parameters --

# image_params.update({"response_format": "b64_json"})  # defaults to "url" for separate download

## -- DALL-E 3 exclusive parameters --
#image_params.update({"model": "dall-e-3"})  # Upgrade the model name to dall-e-3
#image_params.update({"size": "1792x1024"})  # 1792x1024 or 1024x1792 available for DALL-E 3
#image_params.update({"quality": "hd"})      # quality at 2x the price, defaults to "standard" 
#image_params.update({"style": "natural"})   # defaults to "vivid"

# ---- START
# here's the actual request to API and lots of error catching
try:
    images_response = client.images.generate(**image_params)
except openai.APIConnectionError:
    print("Server connection error: {e.__cause__}")  # from httpx.
    raise
except openai.RateLimitError as e:
    print(f"OpenAI RATE LIMIT error {e.status_code}: (e.response)")
    raise
except openai.APIStatusError as e:
    print(f"OpenAI STATUS error {e.status_code}: (e.response)")
    raise
except Exception as e:
    print(f"An unexpected error occurred: {e}")
    raise

# pretty-print images_response
print(images_response.model_dump_json(indent=2)) 

