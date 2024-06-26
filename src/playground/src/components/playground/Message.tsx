import { ChatResponseMessage } from "@azure/openai";
import { makeStyles, shorthands } from "@fluentui/react-components";
import { Person32Regular } from "@fluentui/react-icons";

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
    fontSize: "medium",
    textAlign: "left",
    color: "#000",
    marginRight: "10px",
    boxShadow:
      "0px 0px 4px rgba(0, 0, 0, 0.36), 0px 0px 2px rgba(0, 0, 0, 0.24)",
    backgroundColor: "#fff",
    ...shorthands.padding("12px", "24px"),
    ...shorthands.borderRadius("2px"),
  },
  icon: {
    minWidth:"24px",
    maxWidth:"24px",
    width:"24px",
    marginTop:"6px"
  }
});


export const Message = ({ message }: Props) => {
  const styles = useStyles();

  return (
    <div className={styles.container}>
      <div className={styles.message}>{message.content}</div>
      <Person32Regular className={styles.icon}/>
    </div>
  );
};
