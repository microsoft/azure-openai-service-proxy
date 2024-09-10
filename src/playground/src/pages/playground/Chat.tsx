import { useReducer } from "react";
import { ChatCard } from "../../components/playground/ChatCard";
import { SystemCard } from "../../components/playground/SystemCard";
import { ChatParamsCard } from "../../components/playground/ChatParamsCard";
import { useOpenAIClientContext } from "../../providers/OpenAIProvider";
import type {
  ChatRequestMessage,
  ChatResponseMessage,
  FunctionDefinition,
  GetChatCompletionsOptions,
} from "@azure/openai";
import { makeStyles } from "@fluentui/react-components";
import { reducer } from "./Chat.reducers";
import { INITIAL_STATE } from "./Chat.state";

const useStyles = makeStyles({
  container: {
    textAlign: "center",
    display: "grid",
    gridTemplateColumns: "1.5fr 5.0fr 1fr",
    gridGap: "1px",
  },
});

function mapMessage(message: ChatResponseMessage): ChatRequestMessage {
  const { role, content, toolCalls, functionCall } = message;
  switch (role) {
    case "system":
    case "user":
      return {
        role,
        content: content ?? "",
      };

    case "assistant":
      return {
        role,
        content,
      };

    case "tool":
      return {
        role,
        content,
        toolCallId: toolCalls[0]?.id,
      };

    case "function":
      return {
        role,
        content,
        name: functionCall?.name ?? "auto",
      };
    default:
      throw new Error("Unknown message role");
  }
}

export const Chat = () => {
  const [state, dispatch] = useReducer(reducer, INITIAL_STATE);

  const { client } = useOpenAIClientContext();

  const onPromptEntered = async (prompt: string) => {
    if (!client || !state.model) {
      return;
    }

    dispatch({ type: "chatStart", payload: prompt });
    try {
      const start = Date.now();

      const messages: ChatRequestMessage[] = [
        state.systemPrompt,
        ...state.messages.filter((m) => !m.isError).map(mapMessage),
        { role: "user", content: prompt },
      ];

      const chatCompletions = await client.getChatCompletions(
        state.model,
        messages,
        {
          ...state.params,
          functions: state.params.functions?.filter((f) => f.name),
        }
      );
      const end = Date.now();

      dispatch({
        type: "chatComplete",
        payload: {
          choices: chatCompletions.choices,
          totalTime: end - start,
          completionTokens: chatCompletions.usage?.completionTokens,
          promptTokens: chatCompletions.usage?.promptTokens,
          totalTokens: chatCompletions.usage?.totalTokens,
        },
      });
    } catch (error) {
      dispatch({ type: "chatError", payload: error as unknown });
    }
  };

  const systemPromptChange = (newPrompt: string) => {
    dispatch({ type: "updateSystemMessage", payload: newPrompt });
  };

  const clearMessageList = () => {
    dispatch({ type: "clearMessages" });
  };

  const tokenUpdate = (
    name: keyof GetChatCompletionsOptions | "model",
    value: number | string
  ) => {
    if (name === "functionCall") {
      dispatch({ type: "updateFunctionCall", payload: value as string });
    } else if (name === "model") {
      dispatch({ type: "updateModel", payload: value as string });
    } else {
      dispatch({ type: "updateParameters", payload: { name, value } });
    }
  };

  const functionsChange = (functions: FunctionDefinition[]) =>
    dispatch({ type: "updateFunctions", payload: functions });

  const styles = useStyles();

  return (
    <section className={styles.container}>
      <SystemCard
        defaultPrompt={state.systemPrompt}
        systemPromptChange={systemPromptChange}
        functionsChange={functionsChange}
      />

      <ChatCard
        onPromptEntered={onPromptEntered}
        messageList={state.messages}
        onClear={clearMessageList}
        isLoading={state.isLoading}
        canChat={
          client !== undefined &&
          state.model !== undefined &&
          state.model !== ""
        }
      />

      <ChatParamsCard
        startValues={state.params}
        tokenUpdate={tokenUpdate}
        usageData={state.usageData}
      />
    </section>
  );
};
