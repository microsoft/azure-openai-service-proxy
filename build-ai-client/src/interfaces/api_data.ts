export interface ApiData {
    prompt: string;
    user: string[];
    system: string[];
    assistant: string[];
    max_tokens: number;
    temperature: number;
    top_p: number;
    stop_sequence: string;
    frequency_penalty: number;
    presence_penalty: number;
  }