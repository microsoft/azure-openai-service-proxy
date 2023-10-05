import React, { useState } from 'react';
import './App.css';
import {
  makeStyles,
  Body1,
  Caption1,
  Button,
  shorthands,
  Card,
  CardFooter,
  CardHeader,
  CardPreview,
  Field,
  Input,
  Textarea,
  Text,
} from "@fluentui/react-components";
import { SendRegular } from "@fluentui/react-icons"

import { callApi } from './api/api';
import { Message } from './interfaces/Message';
import { ApiData } from './interfaces/ApiData';
import { ChatCard } from './components/ChatCard';
import { SystemCard } from './components/SystemCard';
import { SlidersCard } from './components/SlidersCard';

const defaultSysPrompt: Message = {role: "system", content: "You are an AI assistant that helps people find information."}
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

  const onPromptEntered = (messages: Message[]) => {
    const data: ApiData = {messages, ...sliders}
    callApi(data).then((response) => {
      console.log(response.assistant.content)
      setMessageList([...messages, response.assistant])
    });
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
      <SystemCard />
      <ChatCard onPromptEntered={onPromptEntered} messageList={messageList}/>
      <SlidersCard startSliders={sliders} tokenUpdate={tokenUpdate} />
    </section>
);
}

export default App;
