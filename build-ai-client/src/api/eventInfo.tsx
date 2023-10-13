export async function eventInfo(eventCode: string): Promise<EventInfo> {
  try {
    const response = await fetch(
      "https://openai-proxy-23uljr-ca.salmonsea-82a61dba.swedencentral.azurecontainerapps.io/api/eventinfo",
      {
        method: "POST",
        headers: {
          accept: "application/json",
          "Content-Type": "application/json",
          "openai-event-code": eventCode,
        },
        body: JSON.stringify(eventCode),
      }
    );
    const data = await response.json();
    return data;
  } catch (error) {
    console.error("Error calling API:", error);
    throw error;
  }
}

export type EventInfo = {
  event_name: string;
  event_url: string;
  event_url_text: string;
  max_token_cap: number;
  is_authorized: boolean;
};
