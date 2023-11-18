import { useState } from "react";
import { ChatCard } from "../components/ChatCard";
import { SystemCard } from "../components/SystemCard";
import { UsageData } from "../interfaces/UsageData";
import { useEventDataContext } from "../EventDataProvider";
import { usePromptErrorContext } from "../PromptErrorProvider";
import { MessageData } from "../interfaces/MessageData";
import { ApiData } from "../interfaces/ApiData";
import { callApi } from "../api/api";
import { ParamsCard } from "../components/ParamsCard";

const defaultSysPrompt: MessageData = {
  role: "system",
  content: "You are an AI assistant that helps people find information.",
};

const defaultParamValues: Omit<ApiData, "messages"> = {
  max_tokens: 512,
  temperature: 0.7,
  top_p: 0.95,
  stop_sequence: "Stop sequences",
  frequency_penalty: 0,
  presence_penalty: 0,
};

export const Chat = () => {
  const [systemPrompt, setSystemPrompt] = useState(defaultSysPrompt);
  const [messageList, setMessageList] = useState([defaultSysPrompt]);

  const [name, setName] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [usageData, setUsageData] = useState<UsageData>({
    finish_reason: "",
    completion_tokens: 0,
    prompt_tokens: 0,
    total_tokens: 0,
    response_time: 0,
  });

  const { eventCode } = useEventDataContext();
  const { setPromptError } = usePromptErrorContext();

  const onPromptEntered = async (messages: MessageData[]) => {
    if (eventCode) {
      const userMessage = messages[messages.length - 1];
      const updatedMessageList = [
        ...messageList,
        { role: "user", content: userMessage.content },
      ];
      setMessageList(updatedMessageList);

      const data: ApiData = { messages: updatedMessageList, ...params };
      setIsLoading(true);
      const { answer, status, error } = await callApi(data, eventCode);
      setPromptError(error);

      if (status !== 200) {
        setMessageList(updatedMessageList.slice(0, 1));
      } else if (answer) {
        setMessageList([...updatedMessageList, answer.assistant]);
        setName(answer.name);
        setUsageData({
          finish_reason: answer.finish_reason,
          completion_tokens: answer.usage.usage.completion_tokens,
          prompt_tokens: answer.usage.usage.prompt_tokens,
          total_tokens: answer.usage.usage.total_tokens,
          response_time: answer.response_ms,
        });
      }
    }

    setIsLoading(false);
  };

  const onPromptChange = (newPrompt: MessageData) => {
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
    label: keyof Omit<ApiData, "messages">,
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
        name={name}
        usageData={usageData}
      />
    </>
  );
};
