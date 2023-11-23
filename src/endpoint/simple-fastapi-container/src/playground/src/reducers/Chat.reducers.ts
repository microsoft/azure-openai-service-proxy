import {
  ChatMessage,
  ChatChoice,
  FunctionDefinition,
  GetChatCompletionsOptions,
} from "@azure/openai";
import { ChatState, INITIAL_STATE } from "./Chat.state";

type ChatAction =
  | {
      type: "chatStart";
      payload: ChatMessage;
    }
  | {
      type: "chatComplete";
      payload: {
        choices: ChatChoice[];
        completionTokens?: number;
        promptTokens?: number;
        totalTokens?: number;
        totalTime: number;
      };
    }
  | {
      type: "chatError";
    }
  | {
      type: "updateSystemMessage";
      payload: ChatMessage;
    }
  | {
      type: "clearMessages";
    }
  | {
      type: "updateFunctions";
      payload: FunctionDefinition[];
    }
  | {
      type: "updateParameters";
      payload: {
        name: keyof GetChatCompletionsOptions;
        value: number | string;
      };
    }
  | {
      type: "updateFunctionCall";
      payload: string;
    };

export function reducer(state: ChatState, action: ChatAction): ChatState {
  switch (action.type) {
    case "chatStart":
      return {
        ...state,
        isLoading: true,
        messages: [...state.messages, action.payload],
      };

    case "chatComplete":
      const {
        choices,
        completionTokens,
        promptTokens,
        totalTime,
        totalTokens,
      } = action.payload;
      let newState = { ...state, isLoading: false };

      const usageData = newState.usageData;

      for (const choice of choices) {
        const message = choice.message;
        if (!message) {
          continue;
        }
        newState = {
          ...newState,
          messages: [...newState.messages, message],
          usageData: {
            ...usageData,
            finish_reason: choice.finishReason || usageData.finish_reason,
            completion_tokens: completionTokens || usageData.completion_tokens,
            prompt_tokens: promptTokens || usageData.prompt_tokens,
            total_tokens: totalTokens || usageData.total_tokens,
            response_time: totalTime,
          },
        };
      }
      return newState;

    case "chatError":
      return { ...state, isLoading: false };

    case "updateSystemMessage":
      const messages = state.messages;
      messages[0] = action.payload;
      return { ...state, messages: [...messages] };

    case "clearMessages":
      return {
        ...state,
        messages: [state.messages[0]],
        usageData: INITIAL_STATE.usageData,
      };

    case "updateFunctions":
      return {
        ...state,
        params: {
          ...state.params,
          functions: action.payload,
        },
      };

    case "updateParameters":
      const { name, value } = action.payload;
      return {
        ...state,
        params: {
          ...state.params,
          [name]: value,
        },
      };

    case "updateFunctionCall":
      return {
        ...state,
        params: {
          ...state.params,
          functionCall: ["auto", "none"].includes(action.payload)
            ? action.payload
            : { name: action.payload },
        },
      };

    default:
      return state;
  }
}
