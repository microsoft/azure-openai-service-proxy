import { makeStyles, Body1, Button, Card, CardFooter, CardHeader, Field, Text, Textarea } from '@fluentui/react-components';
import React, { useEffect, useRef, useState } from 'react';
import { Delete24Regular, SendRegular } from "@fluentui/react-icons"
import { MessageData } from '../interfaces/MessageData';
import { Message } from './Message';
import { Response } from './Response';
import './ChatCard.module.css'

interface CardProps {
  onPromptEntered: (messages: MessageData[]) => void;
  messageList: MessageData[];
  onClear: () => void;
}

const useStyles = makeStyles({
  card: {
    height: "100vh",
    display: "flex",
    marginTop: "10px",
    marginRight: "10px",
    marginBottom: "10px",
    marginLeft: "10px",
  },
  dialog: {
    display: "block"
  },
  smallButton: {
    width: "100%",
    height: "50%",
    maxWidth: "none"
  }
})

export const ChatCard = ({ onPromptEntered, messageList, onClear }: CardProps) => {
  const [userPrompt, setPrompt] = useState("");
  const chat = useStyles();
  const chatContainerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (chatContainerRef.current) {
      chatContainerRef.current.scrollTop = chatContainerRef.current.scrollHeight;
    }
  }, [messageList]);

  return (
    <Card className={chat.card}>
      <CardHeader
        style={{ height: "10vh" }}
        header={
          <Body1 style={{ fontSize: "large" }}>
            <h2>Chat Session</h2>
          </Body1>
        }
      />
      <div id={"chatContainer"} style={{ overflowY: "auto" }} ref={chatContainerRef}>
        {messageList.length > 1 ? messageList.map((message, index) => {

          if (message.role === "system") {
            return null;
          }
          return (
            message.role === "user" ? <Message key={index} message={message} /> : <Response key={index} message={message} />
          )
        }):
          <Text style={{fontSize: "large"}}>Start Chatting</Text>
        }
      </div>
      <CardFooter style={{ height: "10vh" }}>
        <Field className="user-query" style={{ width: "100%" }}>
          <Textarea
            style={{}}
            value={userPrompt}
            placeholder="Type user query here (Shift + Enter for new line)"
            onChange={(event) => {
              setPrompt(event.target.value);
            }}
            onKeyDown={(event) => {
              if (event.key === "Enter" && !event.shiftKey) {
                onPromptEntered([...messageList, { role: "user", content: userPrompt }]);
                setPrompt("");
              }
            }}
          />
        </Field>
        <div>
        <Button className={chat.smallButton} id={"send-button"}
          icon={<SendRegular />} iconPosition='after'
          onClick={() => {
            onPromptEntered([...messageList, { role: "user", content: userPrompt }]);
            setPrompt("");
          }}
        >Send</Button>
        <Button 
        className={chat.smallButton}
        id="clear-button"
        icon={<Delete24Regular />} iconPosition='after'
        onClick={() => {
          onClear();
        }}>Clear Chat</Button>
        </div>
      </CardFooter>
    </Card>
  );
};
