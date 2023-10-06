import { makeStyles,
    Body1,
    Button,
    Card,
    CardFooter,
    CardHeader,
    Field,
    Input,
    Text } from '@fluentui/react-components';
import React from 'react';
import { SendRegular } from "@fluentui/react-icons"
import { useState } from 'react';
import { MessageData } from '../interfaces/MessageData';
import { Message } from './Message';
import { Response } from './Response';

interface CardProps {
    onPromptEntered: (messages: MessageData[]) => void;
    messageList: MessageData[];
}

const useStyles = makeStyles({
    card: {
    //   ...shorthands.margin("auto"),
      height: "100vh",
      width: "100%",
    },
    dialog: {
        display: "block"
    },
  })


export const ChatCard = ({ onPromptEntered, messageList}: CardProps) => {
    const [userPrompt, setPrompt] = useState("");
    const chat = useStyles();
    return (
    <Card className={chat.card}>
    <CardHeader
        style={{height: "10vh"}}
        header={
            <Body1 style={{fontSize: "large"}}>
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
                message.role === "user" ? <Message message={message} />:  <Response message={message} />
                
                // <Text key={index} className={chat.dialog}>
                //     <b>{message.role === "user" ? "You" : "AI"}</b>: {message.content}
                // </Text>
            )
        })
        :
            <Text>Here is where the response will be shown.</Text>
        
        
        }
      </div>
      <CardFooter style={{height: "10vh"}}>
        <Field className="user-query" style={{width: "100%"}}>
          <Input
          style={{fontSize: "large"}}
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
