import React from 'react';
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
import { useState, useEffect } from 'react';
import { callApi } from './api';


const useStyles = makeStyles({
  card: {
    ...shorthands.margin("auto"),
    width: "720px",
    maxWidth: "60%",
  }
})

function App() {

  const [userPrompt, setPrompt] = useState("");
  const data = {
    prompt: "string",
    user: [userPrompt],
    system: ["You are an AI bot that responds in at most two sentences."],
    assistant: ["string"],
    max_tokens: 1024,
    temperature: 0,
    top_p: 0,
    stop_sequence: "",
    frequency_penalty: 0,
    presence_penalty: 0,
  }


  const chat = useStyles();
  return (
    <Card className={chat.card}>
      <CardHeader
        header={
          <Body1>
            <b>Chat Session</b>
          </Body1>
        }
      />
      <div className="chatContainer">
        <Text>Here is where the response will be shown.</Text>
      </div>
      <CardFooter>
        <Field className="user-query" style={{width: "100%"}}>
          <Input 
          value={userPrompt}
          placeholder="Type user query here"
          onChange={(event) => {
            setPrompt(event.target.value);
          }}
          />
        </Field>
        <Button className="send-button" 
        icon={<SendRegular />}
        onClick={() => {
          callApi(data).then((response) => {
            console.log(response);
          })
        }}       
        />
      </CardFooter>
    </Card>
);
}

export default App;
