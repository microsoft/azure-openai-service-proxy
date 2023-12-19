import { API_VERSION } from "../constants";

export async function eventInfo(
  eventCode: string,
  abortController: AbortController
): Promise<EventInfo> {
  try {
    const response = await fetch(`/api/${API_VERSION}/eventinfo`, {
      method: "POST",
      headers: {
        accept: "application/json",
        "Content-Type": "application/json",
        "api-key": eventCode,
      },
      body: JSON.stringify(eventCode),
      signal: abortController.signal,
    });
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
