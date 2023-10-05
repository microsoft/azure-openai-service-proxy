import { Message } from "./Message";

export interface ApiData {
    messages: Message[];
    max_tokens: number;
    temperature: number;
    top_p: number;
    stop_sequence: string;
    frequency_penalty: number;
    presence_penalty: number;
}