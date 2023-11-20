import styles from "./Message.module.css";
import { ChatMessage } from "@azure/openai";

interface Props {
  message: ChatMessage;
}

export const Message = ({ message }: Props) => {
  return (
    <div className={styles.container}>
      <div className={styles.message}>{message.content}</div>
    </div>
  );
};
