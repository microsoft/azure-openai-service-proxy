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
    flexWrap: "wrap", // Enable wrapping for the images
  },
  image: {
    maxWidth: "20%",
    height: "auto",
    ...shorthands.margin("2px") // Add spacing between the images manually
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
    minWidth: "24px",
    maxWidth: "24px",
    width: "24px",
    marginTop: "6px"
  }
});


export const Message = ({ message }: Props) => {
  const styles = useStyles();

  if (Array.isArray(message.content)) {
    // Find the text content if it exists
    const textContent = message.content.find(item => item.type === 'text')?.text;

    // Find all image URLs
    const imageUrls = message.content
      .filter(item => item.type === 'image_url')
      .map(item => item.imageUrl?.url);

    return (
      <div className={styles.container}>
        {textContent && <div className={styles.message}>{textContent}</div>}
        {imageUrls.map((url, index) => (
          <img
            key={index}
            src={url}
            alt={`Message image ${index + 1}`}
            className={styles.image}// Restrict image width
          />
        ))}
        <Person32Regular className={styles.icon} />
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.message}>{message.content}</div>
      <Person32Regular className={styles.icon} />
    </div>
  );
};
