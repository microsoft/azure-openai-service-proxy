import React, { useCallback, useEffect, useState } from 'react';
import './App.css';
import { callApi } from './api/api';
import { MessageData } from './interfaces/MessageData';
import { ApiData } from './interfaces/ApiData';
import { UsageData} from './interfaces/UsageData';
import { ChatCard } from './components/ChatCard';
import { SystemCard } from './components/SystemCard';
import { ParamsCard } from './components/ParamsCard';
import { eventInfo } from './api/eventInfo';
import { EventData } from './interfaces/EventData';


const defaultSysPrompt: MessageData = {role: "system", content: "You are an AI assistant that helps people find information."}
const defaultParamValues: Omit<ApiData, "messages"> = {
  max_tokens: 512,
  temperature: 0.7,
  top_p: 0.95,
  stop_sequence: "Stop sequences",
  frequency_penalty: 0,
  presence_penalty: 0
}
const defaultEventData: EventData = {
  authorized: false,
  max_token_cap: 512,
  event_name: "",
  event_url: "",
  event_url_text: ""
}

function App() {
  const [systemPrompt, setSystemPrompt] = useState(defaultSysPrompt)
  const [messageList, setMessageList] = useState([defaultSysPrompt]);
  const [params, setParams] = useState(defaultParamValues);
  const [name, setName] = useState("");
  const [eventCode, setEventCode] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [usageData, setUsageData] = useState<UsageData>({
    finish_reason: "",
    completion_tokens: 0,
    prompt_tokens: 0,
    total_tokens: 0,
  });
  const [maxTokens, setMaxTokens] = useState<number>(0);
  const [eventData, setEventData] = useState(defaultEventData);

  const onPromptEntered = async (messages: MessageData[]) => {
    if (eventCode !== "") {
      const userMessage = messages[messages.length - 1];
      const updatedMessageList = [...messageList, { role: "user", content: userMessage.content }];
      setMessageList(updatedMessageList);

      const data: ApiData = { messages: updatedMessageList, ...params };
      setIsLoading(true);
      const { answer, status } = await callApi(data, eventCode);
      if(status !== 200) {
        setMessageList(updatedMessageList.slice(0, 1));
        setIsLoading(false);
      } else {   
        setMessageList([...updatedMessageList, answer.assistant]);
        setName(answer.name);
        setUsageData ({
          finish_reason: answer.finish_reason,
          completion_tokens: answer.usage.usage.completion_tokens,
          prompt_tokens: answer.usage.usage.prompt_tokens,
          total_tokens: answer.usage.usage.total_tokens,
      });
        setIsLoading(false);
      }
   
    } else {
      alert("Please enter an event code");
    }
  };

  const onPromptChange = (newPrompt: MessageData) => {
    if(newPrompt !== systemPrompt){
      setSystemPrompt(newPrompt);
      messageList[0] = newPrompt;
    }
  }

  const tokenUpdate = (label: keyof Omit<ApiData, "messages">, newValue: number | string) => {
    setParams(() => {
      return {
        ...params,
        [label]: newValue
      }
    })
    console.log("test");
  }

const clearMessageList = () => {
  setMessageList((prevMessageList) => [prevMessageList[0]]);
};

const eventCodeChange = (newEventCode: string) => {
  setEventCode(newEventCode);
}

const getEventData = useCallback(async () => {
  const data = await eventInfo(eventCode);
  setMaxTokens(data.max_token_cap);
  setEventData(data);
}, [eventCode]);

useEffect(() => {
  getEventData();
}, [eventCode, getEventData]);

  return (
    <section className="App">
        <SystemCard defaultPrompt={systemPrompt} onPromptChange={onPromptChange}/>
        <ChatCard onPromptEntered={onPromptEntered} messageList={messageList} onClear={clearMessageList} 
        isLoading={isLoading} eventName={eventData.event_name}/>
        <ParamsCard startValues={params} tokenUpdate={tokenUpdate} name={name} 
        eventUpdate={eventCodeChange} usageData={usageData} maxTokens={maxTokens} 
        eventData={eventData}/>
    </section>
);
}

export default App;
