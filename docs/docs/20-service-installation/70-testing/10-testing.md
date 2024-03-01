# Proxy service

There are various options to test the endpoint. The simplest is to use Curl from either PowerShell or a Bash/zsh terminal. For example:

From PowerShell 7 and above on Windows, macOS, and Linux:

```pwsh
curl -X 'POST' `
  'https://YOUR_REST_ENDPOINT/api/oai_prompt' `
  -H 'accept: application/json' `
  -H 'Content-Type: application/json' `
  -H 'api-key: API_KEY' `
  -d '{
  "messages": [
    {"role": "system", "content":"What is this about"},
    {"role": "user", "content":"The quick brown fox jumps over the lazy dog"}
  ],
  "max_tokens": 1024,
  "temperature": 0,
  "top_p": 0,
  "stop_sequence": "string",
  "frequency_penalty": 0,
  "presence_penalty": 0
}'  | ConvertFrom-Json | ConvertTo-Json
```

From Bash/zsh on macOS, Linux, and Windows WSL:

```bash
curl -X 'POST' \
  'https://YOUR_REST_ENDPOINT/api/oai_prompt' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -H 'api-key: API_KEY' \
  -d '{
  "messages": [
    {"role": "system", "content":"What is this about"},
    {"role": "user", "content":"The quick brown fox jumps over the lazy dog"}
  ],
  "max_tokens": 1024,
  "temperature": 0,
  "top_p": 0,
  "stop_sequence": "string",
  "frequency_penalty": 0,
  "presence_penalty": 0
}'
```

Better still, pip or brew install `jq` to pretty print the JSON response.

```bash
curl -X 'POST' \
  'https://YOUR_REST_ENDPOINT/api/oai_prompt' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -H 'api-key: API_KEY' \
  -d '{
  "messages": [
    {"role": "system", "content":"What is this about"},
    {"role": "user", "content":"The quick brown fox jumps over the lazy dog"}
  ],
  "max_tokens": 1024,
  "temperature": 0,
  "top_p": 0,
  "stop_sequence": "string",
  "frequency_penalty": 0,
  "presence_penalty": 0
}' | jq "."
```
