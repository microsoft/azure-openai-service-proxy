import { Button, Input, Label, makeStyles } from "@fluentui/react-components";
import { useState } from "react";
import { useEventDataContext } from "../providers/EventDataProvider";
import { DividerBlock } from "./DividerBlock";

const useStyles = makeStyles({
  smallbutton: {
    width: "100%",
    height: "50%",
    maxWidth: "none",
    maxHeight: "25%",
    backgroundColor: "#f2f2f2",
  },
});

export const EventCodeInput = () => {
  const styles = useStyles();
  const { eventData, isAuthorized, setEventCode, eventCode } =
    useEventDataContext();
  const [code, setCode] = useState(eventCode || "");

  return (
    <DividerBlock>
      <Label
        style={{
          fontSize: "medium",
          marginBottom: "0.5rem",
          textAlign: "justify",
        }}
      >
        <b>Event Code</b>
      </Label>
      <Input
        type="password"
        placeholder="Enter your Event Code"
        value={code}
        onChange={(e) => setCode(e.target.value)}
        style={{ textAlign: "right" }}
      />

      {!isAuthorized && (
        <>
          <Button
            className={styles.smallbutton}
            onClick={() => setEventCode(code)}
          >
            Log In
          </Button>
          <Label
            style={{
              color: "GrayText",
              fontSize: "small",
              textAlign: "justify",
            }}
          >
            Provided by workshop host.
          </Label>
        </>
      )}
      {isAuthorized && (
        <Label
          style={{
            color: "GrayText",
            fontSize: "small",
            textAlign: "justify",
          }}
        >
          <div>{eventData!.name}</div>
          <div>
            <a href={eventData!.url} target="_blank" rel="noopener noreferrer">
              {eventData!.url_text}
            </a>
          </div>
        </Label>
      )}
    </DividerBlock>
  );
};
