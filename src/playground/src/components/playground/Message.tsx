import { ChatResponseMessage } from "@azure/openai";
import { makeStyles, shorthands } from "@fluentui/react-components";

interface Props {
  message: ChatResponseMessage;
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

  return (
    <div className={styles.container}>
      <div className={styles.message}>{message.content}</div>
    </div>
  );
};
