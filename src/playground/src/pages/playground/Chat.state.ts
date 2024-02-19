import { ChatMessage, GetChatCompletionsOptions } from "@azure/openai";
import { UsageData } from "../../interfaces/UsageData";

const defaultSysPrompt: ChatMessage = {
  role: "system",
  content: "You are an AI assistant that helps people find information.",
};

export type ChatState = {
  isLoading: boolean;
  params: GetChatCompletionsOptions;
  usageData: UsageData;
  messages: ChatMessage[];
  model?: string;
};

export const INITIAL_STATE: ChatState = {
  isLoading: false,
  params: {
    maxTokens: 512,
    temperature: 0.7,
    topP: 0.95,
    stop: ["Stop sequences"],
    frequencyPenalty: 0,
    presencePenalty: 0,
  },
  usageData: {
    finish_reason: "",
    completion_tokens: 0,
    prompt_tokens: 0,
    total_tokens: 0,
    response_time: 0,
  },
  messages: [defaultSysPrompt],
};
