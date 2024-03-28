import {
  ChatRequestSystemMessage,
  ChatResponseMessage,
  GetChatCompletionsOptions,
} from "@azure/openai";
import { UsageData } from "../../interfaces/UsageData";

const defaultSysPrompt: ChatRequestSystemMessage = {
  role: "system",
  content: "You are an AI assistant that helps people find information.",
};

export type ChatResponseMessageExtended = ChatResponseMessage & {
  isError: boolean;
};

export type ChatState = {
  isLoading: boolean;
  params: GetChatCompletionsOptions;
  usageData: UsageData;
  model?: string;
  messages: ChatResponseMessageExtended[];
  systemPrompt: ChatRequestSystemMessage;
};

export const INITIAL_STATE: ChatState = {
  isLoading: false,
  params: {
    maxTokens: 512,
    temperature: 0.7,
    topP: 0.9,
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
  messages: [],
  systemPrompt: defaultSysPrompt,
};
