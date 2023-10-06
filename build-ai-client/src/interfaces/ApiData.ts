import { MessageData } from "./MessageData";

export interface ApiData {
    messages: MessageData[];
    max_tokens: number;
    temperature: number;
    top_p: number;
    stop_sequence: string;
    frequency_penalty: number;
    presence_penalty: number;
}