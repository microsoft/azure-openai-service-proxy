import { makeStyles,
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
    Text } from '@fluentui/react-components';
import React from 'react';
import { SendRegular } from "@fluentui/react-icons"

import { useState, useEffect } from 'react';
import { Message } from '../interfaces/Message';

interface CardProps {
    onPromptEntered: (messages: Message[]) => void;
    messageList: Message[];
}

const useStyles = makeStyles({
    card: {
      ...shorthands.margin("auto"),
      height: "100vh",
      width: "720px",
      maxWidth: "60%",
    },
    dialog: {
        display: "block"
    }
  })


export const ChatCard = ({ onPromptEntered, messageList}: CardProps) => {
    const [userPrompt, setPrompt] = useState("");
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
        {messageList.length > 1 ? messageList.map((message, index) => {
            if (message.role === "system") {
                return null;
            }
            return (
                <Text key={index} className={chat.dialog}>
                    <b>{message.role === "user" ? "You" : "AI"}</b>: {message.content}
                </Text>
            )
        })
        :
            <Text>Here is where the response will be shown.</Text>
        
        
        }
      </div>
      <CardFooter>
        <Field className="user-query" style={{width: "100%"}}>
          <Input 
          value={userPrompt}
          placeholder="Type user query here"
          onChange={(event) => {
            setPrompt(event.target.value);
          }}
          onKeyDown={(event) => {
            if (event.key === "Enter") {
              onPromptEntered([...messageList, {role: "user", content: userPrompt}]);
              setPrompt("");
            }
          }}
          />
        </Field>
        <Button className="send-button" 
        icon={<SendRegular />}
        onClick={() => {
            onPromptEntered([...messageList, {role: "user", content: userPrompt}]);
            setPrompt("");
        }}       
        />
      </CardFooter>
    </Card>
    );
};
