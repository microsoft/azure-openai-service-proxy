import styles from "./Response.module.css";
import Markdown from "react-markdown";
import { ChatMessage } from "@azure/openai";

interface Props {
  message: ChatMessage;
}

export const Response = ({ message }: Props) => {
  return (
    <div className={styles.container}>
      <div className={styles.response}>
        <Markdown>{message.content}</Markdown>
      </div>
    </div>
  );
};
