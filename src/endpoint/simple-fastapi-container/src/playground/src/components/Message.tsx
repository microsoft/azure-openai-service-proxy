import { MessageData } from "../interfaces/MessageData";
import styles from "./Message.module.css";

interface Props {
    message: MessageData;
}

export const Message = ({ message }: Props) => {
    return (
        <div className={styles.container}>
            <div className={styles.message}>{message.content}</div>
        </div>
    );
};