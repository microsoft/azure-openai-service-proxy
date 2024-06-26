import {
  Text,
  makeStyles,
  shorthands,
  mergeClasses,
} from "@fluentui/react-components";
import { Delete24Regular } from "@fluentui/react-icons";

const useStyles = makeStyles({
  container: {
    display: "flex",
    alignItems: "center",
    ...shorthands.gap("6px"),
    cursor: "pointer",
  },
  disabled: {
    opacity: 0.4,
  },
});

interface Props {
  className?: string;
  onClick: () => void;
  disabled?: boolean;
}

export const ClearChatButton = ({ className, disabled, onClick }: Props) => {
  const styles = useStyles();
  return (
    <div
      className={mergeClasses(
        styles.container,
        className,
        disabled ? styles.disabled : ""
      )}
      onClick={onClick}
    >
      <Delete24Regular />
      <Text>{"Clear"}</Text>
    </div>
  );
};
