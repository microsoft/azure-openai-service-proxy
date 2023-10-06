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
  stop_sequence: "string",
  frequency_penalty: 0,
  presence_penalty: 0
}


function App() {
  const [systemPrompt, setSystemPrompt] = useState(defaultSysPrompt)
  const [messageList, setMessageList] = useState([defaultSysPrompt]);
  const [sliders, setSliders] = useState(defaultSliders);

  const onPromptEntered = (messages: MessageData[]) => {
    const data: ApiData = {messages, ...sliders}
    callApi(data).then((response) => {
      console.log(response.assistant.content)
      setMessageList([...messages, response.assistant])
    });
  }

  const onPromptChange = (newPrompt: MessageData) => {
    setSystemPrompt(newPrompt);
    messageList[0] = newPrompt;
  }

  const tokenUpdate = (label: keyof Omit<ApiData, "messages">, newValue: number | string) => {
    setSliders(() => {
      return {
        ...sliders,
        [label]: newValue
      }
    })
  }


  return (
    <section className="App">
      <SystemCard defaultPrompt={systemPrompt} onPromptChange={onPromptChange}/>
      <ChatCard onPromptEntered={onPromptEntered} messageList={messageList}/>
      <SlidersCard startSliders={sliders} tokenUpdate={tokenUpdate} />
    </section>
);
}

export default App;
