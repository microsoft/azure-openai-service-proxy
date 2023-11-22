import { ChatMessage } from "@azure/openai";
import { makeStyles, shorthands } from "@fluentui/react-components";
import SyntaxHighlighter from "react-syntax-highlighter";
import { solarizedlight } from "react-syntax-highlighter/dist/esm/styles/prism";

interface Props {
  message: ChatMessage;
}

const useStyles = makeStyles({
  container: {
    display: "flex",
    justifyContent: "flex-end",
    marginBottom: "20px",
    maxWidth: "80%",
    marginLeft: "auto",
  },
  message: {
    fontSize: "large",
    textAlign: "right",
    color: "white",
    boxShadow:
      "0px 2px 4px rgba(0, 0, 0, 0.14), 0px 0px 2px rgba(0, 0, 0, 0.12)",
    backgroundColor: "#0078D4",
    ...shorthands.padding("20px"),
    ...shorthands.borderRadius("8px"),
    ...shorthands.outline("transparent solid 1px"),
  },
});

export const Message = ({ message }: Props) => {
  const styles = useStyles();

  if (message.content) {
    return (
      <div className={styles.container}>
        <div className={styles.message}>{message.content}</div>
      </div>
    );
  }

  if (message.functionCall) {
    return (
      <div className={styles.container}>
        <div className={styles.message}>
          <h3>Function Call</h3>
          <SyntaxHighlighter language="json" style={solarizedlight}>
            {JSON.stringify(message.functionCall, null, 2)}
          </SyntaxHighlighter>
        </div>
      </div>
    );
  }

  return null;
};
