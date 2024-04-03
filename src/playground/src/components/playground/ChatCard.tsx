import {
  makeStyles,
  Body1,
  Button,
  CardFooter,
  Field,
  Textarea,
  Spinner,
  shorthands
} from "@fluentui/react-components";
import { Dispatch, useEffect, useRef, useState } from "react";
import { Delete24Regular, SendRegular } from "@fluentui/react-icons";
import { Message } from "./Message";
import { Response } from "./Response";
import { useEventDataContext } from "../../providers/EventDataProvider";
import { Card } from "./Card";
import { ChatResponseMessageExtended } from "../../pages/playground/Chat.state";

interface CardProps {
  onPromptEntered: Dispatch<string>;
  messageList: ChatResponseMessageExtended[];
  onClear: () => void;
  isLoading: boolean;
  canChat: boolean;
}

const useStyles = makeStyles({
  dialog: {
    display: "block",
  },
  smallButton: {
    width: "100%",
    height: "40%",
    maxWidth: "none",
    textAlign: "left",
    marginBottom: "12px"
  },
  startCard: {
    display: "flex",
    maxWidth: "80%",
    ...shorthands.margin("35%", "20%"),
    ...shorthands.padding("20px", "0px"),
  },
  chatCard: {
    display: "flex",
    height: "calc(100vh - 92px)",
  },
});

export const ChatCard = ({
  onPromptEntered,
  messageList,
  onClear,
  isLoading,
  canChat,
}: CardProps) => {
  const chat = useStyles();
  const chatContainerRef = useRef<HTMLDivElement>(null);
  const { isAuthorized } = useEventDataContext();

  useEffect(() => {
    if (chatContainerRef.current) {
      chatContainerRef.current.scrollTop =
        chatContainerRef.current.scrollHeight;
    }
  }, [messageList]);

  return (
    <Card header="Chat session" className={chat.chatCard} >

      {isAuthorized && (
        <>
          <div
            id={"chatContainer"}
            style={{ overflowY: "auto" }}
            ref={chatContainerRef}
          >

            {messageList.length > 1 ? (
              messageList.map((message, index) => {
                if (message.role === "system") {
                  return null;
                }
                return message.role === "user" ? (
                  <Message key={index} message={message} />
                ) : (
                  <Response key={index} message={message} />
                );
              })
            ) : (
              <Card className={chat.startCard}>
                <Body1 style={{ textAlign: "center" }}>
                  {!canChat && (<h2>Select a model</h2>)}
                  {canChat && (
                    <>
                      <h2>Start chatting</h2>
                      Test your assistant by sending queries below. Then adjust your assistant setup to improve the assistant's responses.
                    </>
                  )}
                </Body1>
              </Card>
            )}
          </div>
          {isLoading && <Spinner />}
          {isAuthorized && (
            <ChatInput
              promptSubmitted={onPromptEntered}
              onClear={onClear}
              canChat={canChat}
            />
          )}
          {!isAuthorized && (
            <>
              <CardFooter style={{ height: "10vh" }}>
                <p>Please enter your event code to start the chat session.</p>
              </CardFooter>
            </>
          )}
        </>
      )}
    </Card>
  );
};

const hasPrompt = (prompt: string) => {
  const regex = /^\s*$/;
  return !regex.test(prompt);
};

function ChatInput({
  promptSubmitted,
  onClear,
  canChat,
}: {
  promptSubmitted: Dispatch<string>;
  onClear: () => void;
  canChat: boolean;
}) {
  const [userPrompt, setPrompt] = useState("");

  const chat = useStyles();
  return (
    <CardFooter style={{ height: "10vh" }}>
      <Field className="user-query" style={{ width: "100%" }}>
        <Textarea
          value={userPrompt}
          placeholder="Type user query here (Shift + Enter for new line)"
          disabled={!canChat}
          onChange={(event) => setPrompt(event.target.value)}
          onKeyDown={(event) => {
            if (
              event.key === "Enter" &&
              !event.shiftKey &&
              hasPrompt(userPrompt)
            ) {
              promptSubmitted(userPrompt);
              setPrompt("");
              event.preventDefault();
            }
          }}
        />
      </Field>
      <div>
        <Button
          className={chat.smallButton}
          id={"send-button"}
          icon={<SendRegular />}
          iconPosition="before"
          appearance="primary"
          onClick={() => {
            promptSubmitted(userPrompt);
            setPrompt("");
          }}
          disabled={!canChat || !hasPrompt(userPrompt)}
        >
          Send
        </Button>
        <Button
          className={chat.smallButton}
          id="clear-button"
          disabled={!canChat}
          icon={<Delete24Regular />}
          iconPosition="before"
          onClick={onClear}
        >
          Clear
        </Button>
      </div>
    </CardFooter>
  );
}
