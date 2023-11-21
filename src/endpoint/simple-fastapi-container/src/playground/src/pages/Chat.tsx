import { useState } from "react";
import { ChatCard } from "../components/ChatCard";
import { SystemCard } from "../components/SystemCard";
import { usePromptErrorContext } from "../providers/PromptErrorProvider";
import { ParamsCard } from "../components/ChatParamsCard";
import { useOpenAIClientContext } from "../providers/OpenAIProvider";
import type { ChatMessage, GetChatCompletionsOptions } from "@azure/openai";

type UsageData = {
  finish_reason: string;
  completion_tokens: number;
  prompt_tokens: number;
  total_tokens: number;
  response_time: number;
};

const defaultSysPrompt: ChatMessage = {
  role: "system",
  content: "You are an AI assistant that helps people find information.",
};

const defaultParamValues: GetChatCompletionsOptions = {
  maxTokens: 512,
  temperature: 0.7,
  topP: 0.95,
  stop: ["Stop sequences"],
  frequencyPenalty: 0,
  presencePenalty: 0,
};

export const Chat = () => {
  const [systemPrompt, setSystemPrompt] = useState(defaultSysPrompt);
  const [messageList, setMessageList] = useState([defaultSysPrompt]);

  const [isLoading, setIsLoading] = useState(false);
  const [usageData, setUsageData] = useState<UsageData>({
    finish_reason: "",
    completion_tokens: 0,
    prompt_tokens: 0,
    total_tokens: 0,
    response_time: 0,
  });

  const { setPromptError } = usePromptErrorContext();
  const { client } = useOpenAIClientContext();

  const onPromptEntered = async (messages: ChatMessage[]) => {
    if (client) {
      const userMessage = messages[messages.length - 1];
      const updatedMessageList = [
        ...messageList,
        { role: "user", content: userMessage.content },
      ];
      setMessageList(updatedMessageList);

      setIsLoading(true);
      try {
        const start = Date.now();
        const chatCompletions = await client.getChatCompletions(
          "proxy",
          updatedMessageList.map((m) => ({
            content: m.content,
            role: m.role,
          })),
          params
        );
        const end = Date.now();

        const totalTime = end - start;

        const choices = chatCompletions.choices;

        for (const choice of choices) {
          const message = choice.message;
          if (!message) {
            continue;
          }
          setMessageList((current) => [...current, message]);

          setUsageData((current) => ({
            ...current,
            finish_reason: choice.finishReason || current.finish_reason,
            completion_tokens:
              chatCompletions.usage?.completionTokens ||
              current.completion_tokens,
            prompt_tokens:
              chatCompletions.usage?.promptTokens || current.prompt_tokens,
            total_tokens:
              chatCompletions.usage?.totalTokens || current.total_tokens,
            response_time: totalTime,
          }));
        }

        setIsLoading(false);
      } catch (error) {
        setMessageList(updatedMessageList.slice(0, 1));
        setPromptError(error + "");

        setIsLoading(false);
      }
    }
  };

  const onPromptChange = (newPrompt: ChatMessage) => {
    if (newPrompt !== systemPrompt) {
      setSystemPrompt(newPrompt);
      messageList[0] = newPrompt;
    }
  };

  const clearMessageList = () => {
    setMessageList((prevMessageList) => [prevMessageList[0]]);
    setUsageData({
      finish_reason: "",
      completion_tokens: 0,
      prompt_tokens: 0,
      total_tokens: 0,
      response_time: 0,
    });
  };

  const [params, setParams] = useState(defaultParamValues);

  const tokenUpdate = (
    label: keyof GetChatCompletionsOptions,
    newValue: number | string
  ) => {
    setParams((params) => {
      return {
        ...params,
        [label]: newValue,
      };
    });
  };

  return (
    <>
      <SystemCard
        defaultPrompt={systemPrompt}
        onPromptChange={onPromptChange}
      />

      <ChatCard
        onPromptEntered={onPromptEntered}
        messageList={messageList}
        onClear={clearMessageList}
        isLoading={isLoading}
      />

      <ParamsCard
        startValues={params}
        tokenUpdate={tokenUpdate}
        usageData={usageData}
      />
    </>
  );
};
