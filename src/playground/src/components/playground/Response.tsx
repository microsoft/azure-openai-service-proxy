import Markdown from "react-markdown";
import { ChatResponseMessage } from "@azure/openai";
import { makeStyles, shorthands } from "@fluentui/react-components";
import SyntaxHighlighter from "react-syntax-highlighter";
import { solarizedlight } from "react-syntax-highlighter/dist/esm/styles/prism";
import { Bot24Regular } from "@fluentui/react-icons";

interface Props {
  message: ChatResponseMessage;
}

const useStyles = makeStyles({
  container: {
    display: "flex",
    justifyContent: "flex-start",
    marginBottom: "20px",
    maxWidth: "80%",
  },
  response: {
    fontSize: "medium",
    textAlign: "left",
    color: "#000",
    marginLeft: "12px",
    backgroundColor: "#fff",
    boxShadow: "0px 0px 4px rgba(0, 0, 0, 0.36), 0px 0px 2px rgba(0, 0, 0, 0.24)",
    ...shorthands.padding("0px", "24px"),
    ...shorthands.borderRadius("2px"),
    ...shorthands.outline("transparent solid 1px"),
  },
  markdown: {
    paddingTop: "0px",
    marginTop: "0px"
  },
  icon: {
    minWidth:"24px",
    maxWidth:"24px",
    width:"auto",
    marginTop:"6px"
  }
});

export const Response = ({ message }: Props) => {
  const styles = useStyles();
  if (message.content) {
    return (
      <div className={styles.container}>
        <Bot24Regular className={styles.icon}/>
        <div className={styles.response}>
          <Markdown className={styles.markdown}>{message.content}</Markdown>
        </div>
      </div>
    );
  }

  if (message.functionCall) {
    return (
      <div className={styles.container}>
        <div className={styles.response}>
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
