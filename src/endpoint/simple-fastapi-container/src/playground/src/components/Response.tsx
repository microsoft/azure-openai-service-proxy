import { MessageData } from "../interfaces/MessageData";
import styles from "./Response.module.css";
import Markdown from "react-markdown";

interface Props {
    message: MessageData;
}

export const Response = ({ message }: Props) => {
    return (
        <div className={styles.container}>
            <div className={styles.response}>
                <Markdown>{message.content}</Markdown></div>
        </div>
    );
};