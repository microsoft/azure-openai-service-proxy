import {
  ChatMessage,
  ChatChoice,
  FunctionDefinition,
  GetChatCompletionsOptions,
} from "@azure/openai";
import { ChatState, INITIAL_STATE } from "./Chat.state";

const findRAIError = (
  error: Record<string, { filtered: boolean; severity: number }>
): string => {
  const keys = Object.keys(error);

  for (const key of keys) {
    if (error[key].filtered) {
      return `${key.charAt(0).toUpperCase() + key.slice(1)} (${
        error[key].severity
      })`;
    }
  }

  return "Unknown error";
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const createErrorMessage = (error: any): string => {
  const innerError = error.innererror;

  if (!innerError) {
    const message = error.message;

    if (message) {
      return message;
    }

    console.error(error);
    return "The error does not match a known format, check the console.error output for more info";
  }

  switch (innerError.code) {
    case "ResponsibleAIPolicyViolation":
      return `The prompt was filtered due to triggering Azure OpenAI's content filtering system.

**Reason**: This prompt contains content flagged as **${findRAIError(
        innerError.content_filter_result
      )}**.`;

    default:
      return `An error occurred: ${error.message}`;
  }
};

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
      payload: unknown;
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
    }
  | {
      type: "updateModel";
      payload: string;
    };

export function reducer(state: ChatState, action: ChatAction): ChatState {
  switch (action.type) {
    case "chatStart":
      return {
        ...state,
        isLoading: true,
        messages: [...state.messages, { ...action.payload, isError: false }],
      };

    case "chatComplete": {
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
          messages: [...newState.messages, { ...message, isError: false }],
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
    }

    case "chatError": {
      const error = action.payload;
      const errorMessage: ChatMessage = {
        role: "assistant",
        content: createErrorMessage(error),
      };
      return {
        ...state,
        isLoading: false,
        messages: [...state.messages, { ...errorMessage, isError: true }],
      };
    }

    case "updateSystemMessage": {
      const messages = state.messages;
      messages[0] = { ...action.payload, isError: false };
      return { ...state, messages: [...messages] };
    }

    case "clearMessages": {
      return {
        ...state,
        messages: [state.messages[0]],
        usageData: INITIAL_STATE.usageData,
      };
    }

    case "updateFunctions":
      return {
        ...state,
        params: {
          ...state.params,
          functions: action.payload,
        },
      };

    case "updateParameters": {
      const { name, value } = action.payload;
      return {
        ...state,
        params: {
          ...state.params,
          [name]: value,
        },
      };
    }

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

    case "updateModel":
      return {
        ...state,
        model: action.payload,
      };

    default:
      return state;
  }
}
