import { useReducer } from "react";
import { ChatCard } from "../components/ChatCard";
import { SystemCard } from "../components/SystemCard";
import { ChatParamsCard } from "../components/ChatParamsCard";
import { useOpenAIClientContext } from "../providers/OpenAIProvider";
import type {
  ChatMessage,
  FunctionDefinition,
  GetChatCompletionsOptions,
} from "@azure/openai";
import { makeStyles } from "@fluentui/react-components";
import { reducer } from "../reducers/Chat.reducers";
import { INITIAL_STATE } from "../reducers/Chat.state";

const useStyles = makeStyles({
  container: {
    textAlign: "center",
    display: "grid",
    gridTemplateColumns: "1.5fr 2.5fr 1fr",
    gridGap: "1px",
  },
});

export const Chat = () => {
  const [state, dispatch] = useReducer(reducer, INITIAL_STATE);

  const { client } = useOpenAIClientContext();

  const onPromptEntered = async (messages: ChatMessage[]) => {
    if (client) {
      dispatch({ type: "chatStart", payload: messages[messages.length - 1] });
      try {
        const start = Date.now();

        const chatCompletions = await client.getChatCompletions(
          "proxy",
          messages.filter((m) => m.content),
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
        dispatch({ type: "chatError", payload: error as any });
      }
    }
  };

  const onPromptChange = (newPrompt: ChatMessage) => {
    dispatch({ type: "updateSystemMessage", payload: newPrompt });
  };

  const clearMessageList = () => {
    dispatch({ type: "clearMessages" });
  };

  const tokenUpdate = (
    name: keyof GetChatCompletionsOptions,
    value: number | string
  ) => {
    if (name === "functionCall") {
      dispatch({ type: "updateFunctionCall", payload: value as string });
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
        defaultPrompt={state.messages[0]}
        onPromptChange={onPromptChange}
        functionsChange={functionsChange}
      />

      <ChatCard
        onPromptEntered={onPromptEntered}
        messageList={state.messages}
        onClear={clearMessageList}
        isLoading={state.isLoading}
      />

      <ChatParamsCard
        startValues={state.params}
        tokenUpdate={tokenUpdate}
        usageData={state.usageData}
      />
    </section>
  );
};
