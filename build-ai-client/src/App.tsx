import React, { useState } from 'react';
import './App.css';
import { callApi } from './api/api';
import { MessageData } from './interfaces/MessageData';
import { ApiData } from './interfaces/ApiData';
import { ChatCard } from './components/ChatCard';
import { SystemCard } from './components/SystemCard';
import { SlidersCard } from './components/SlidersCard';

const defaultSysPrompt: MessageData = {role: "system", content: "You are an AI assistant that helps people find information."}
const defaultSliders: Omit<ApiData, "messages"> = {
  max_tokens: 1024,
  temperature: 0,
  top_p: 0,
  stop_sequence: "Stop sequences",
  frequency_penalty: 0,
  presence_penalty: 0
}

function App() {
  const [systemPrompt, setSystemPrompt] = useState(defaultSysPrompt)
  const [messageList, setMessageList] = useState([defaultSysPrompt]);
  const [sliders, setSliders] = useState(defaultSliders);
  const [name, setName] = useState("");
  const [eventCode, setEventCode] = useState("");

  const onPromptEntered = async (messages: MessageData[]) => {
    const userMessage = messages[messages.length - 1];
    const updatedMessageList = [...messageList, { role: "user", content: userMessage.content }];
    setMessageList(updatedMessageList);

    const data: ApiData = { messages: updatedMessageList, ...sliders };
    const response = await callApi(data, eventCode);
    setMessageList([...updatedMessageList, response.assistant]);
    setName(response.name);
  };

  const onPromptChange = (newPrompt: MessageData) => {
    if(newPrompt !== systemPrompt){
      setSystemPrompt(newPrompt);
      messageList[0] = newPrompt;
    }
  }

  const tokenUpdate = (label: keyof Omit<ApiData, "messages">, newValue: number | string) => {
    setSliders(() => {
      return {
        ...sliders,
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

  return (
    <section className="App">
        <SystemCard defaultPrompt={systemPrompt} onPromptChange={onPromptChange}/>
        <ChatCard onPromptEntered={onPromptEntered} messageList={messageList} onClear={clearMessageList}/>
        <SlidersCard startSliders={sliders} tokenUpdate={tokenUpdate} name={name} eventUpdate={eventCodeChange} />
    </section>
);
}

export default App;
