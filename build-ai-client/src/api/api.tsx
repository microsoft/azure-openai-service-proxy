interface ApiResponse {
  assistant: {
    role: string;
    content: string;
  };
  finish_reason: string;
  response_ms: number;
  content_filtered: {
    hate: {
      filtered: boolean;
      severity: string;
    };
    self_harm: {
      filtered: boolean;
      severity: string;
    };
    sexual: {
      filtered: boolean;
      severity: string;
    };
  };
  usage: {
    usage: {
      completion_tokens: number;
      prompt_tokens: number;
      total_tokens: number;
      max_tokens: number;
    };
  };
  name: string;
}

export async function callApi(
  data: any,
  eventCode: string
): Promise<{ answer?: ApiResponse; status: number; error?: string }> {
  try {
    const response = await fetch(
      "https://openai-proxy-23uljr-ca.salmonsea-82a61dba.swedencentral.azurecontainerapps.io/api/oai_prompt",
      {
        method: "POST",
        headers: {
          accept: "application/json",
          "Content-Type": "application/json",
          "openai-event-code": eventCode,
        },
        body: JSON.stringify(data),
      }
    );
    if (response.status === 200) {
      const responseanswer = await response.json();
      return { answer: responseanswer, status: response.status };
    }

    if (response.status === 401) {
      const responseanswer = await response.json();
      return {
        answer: responseanswer,
        status: response.status,
        error: `Unauthorized: ${response.status}`,
      };
    }

    if (
      response.status === 400 ||
      response.status === 404 ||
      response.status === 415
    ) {
      const responseanswer = await response.json();
      return {
        answer: responseanswer,
        status: response.status,
        error: `Bad Request: ${responseanswer.assistant.content}`,
      };
    }

    if (response.status === 403) {
      const responseanswer = await response.json();
      return {
        answer: responseanswer,
        status: response.status,
        error: `Permission Error: ${response.status}`,
      };
    }

    if (response.status === 409) {
      const responseanswer = await response.json();
      return {
        answer: responseanswer,
        status: response.status,
        error: `Try Again: ${response.status}`,
      };
    }

    if (response.status === 429) {
      const responseanswer = await response.json();
      return {
        answer: responseanswer,
        status: response.status,
        error: `Rate Limited: ${response.status}`,
      };
    }

    if (response.status === 500) {
      const responseanswer = await response.json();
      return {
        answer: responseanswer,
        status: response.status,
        error: `Internal Server Error: ${response.status}`,
      };
    }

    const responseanswer = await response.json();
    return {
      answer: responseanswer,
      status: response.status,
      error: `HTTP error! status: ${response.status}`,
    };
  } catch (error) {
    console.error("Error calling API:", error);
    return {
      answer: undefined,
      status: 500,
      error: `Error calling API: ${error}`,
    };
  }
}
