import {
  Button,
  Input,
  Label,
  makeStyles,
  shorthands,
  useId,
} from "@fluentui/react-components";
import { useState } from "react";
import { useEventDataContext } from "../../../providers/EventDataProvider";

const useStyles = makeStyles({
  container: {
    display: "flex",
    flexDirection: "row",
    columnGap: "5px",
    alignItems: "center",
    ...shorthands.padding("10px", 0, 0, 0),
  },
});

export const ApiKeyInput = () => {
  const [code, setCode] = useState("");
  const { isAuthorized, setEventCode } = useEventDataContext();
  const inputId = useId();
  const styles = useStyles();

  return (
    <div className={styles.container}>
      {!isAuthorized && (
        <>
          <Label htmlFor={inputId}>API Key</Label>
          <Input
            type="password"
            placeholder="Enter your API Key"
            value={code}
            onChange={(e) => setCode(e.target.value)}
            id={inputId}
          />

          <Button
            onClick={(e) => {
              e.preventDefault();
              return setEventCode(code);
            }}
            appearance="primary"
            disabled={code.length === 0}
          >
            Authorize
          </Button>
        </>
      )}
      {isAuthorized && (
        <>
          <Button
            onClick={(e) => {
              e.preventDefault();
              setCode("");
              return setEventCode("");
            }}
            appearance="primary"
          >
            Logout
          </Button>
        </>
      )}
    </div>
  );
};
