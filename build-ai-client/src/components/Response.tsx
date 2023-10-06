import { MessageData } from "../interfaces/MessageData";
import styles from "./Response.module.css";

interface Props {
    message: MessageData;
}

export const Response = ({ message }: Props) => {
    return (
        <div className={styles.container}>
            <div className={styles.response}>{message.content}</div>
        </div>
    );
};