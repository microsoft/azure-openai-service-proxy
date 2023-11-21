import {
  Body1,
  Button,
  Divider,
  Textarea,
  makeStyles,
} from "@fluentui/react-components";
import { useEffect, useState } from "react";
import { Save24Regular } from "@fluentui/react-icons";
import type { ChatMessage } from "@azure/openai";
import { Card } from "./Card";

interface SystemProps {
  defaultPrompt: ChatMessage;
  onPromptChange: (newPrompt: ChatMessage) => void;
}

const useStyles = makeStyles({
  wrapper: {
    display: "flex",
    flexDirection: "column",
    rowGap: "15px",
  },
});

export const SystemCard = ({ defaultPrompt, onPromptChange }: SystemProps) => {
  const [sysPrompt, setPrompt] = useState(defaultPrompt.content || "");
  const [isSaved, setSaved] = useState(false);
  const styles = useStyles();

  useEffect(() => {
    let timeout: NodeJS.Timeout;
    if (isSaved) {
      timeout = setTimeout(() => {
        setSaved(false);
      }, 1500);
    }
    return () => clearTimeout(timeout);
  }, [isSaved]);

  return (
    <Card header="System Message">
      <div style={{ height: "100%" }}>
        <Textarea
          className="test"
          style={{ height: "12.5%", maxHeight: "30%", width: "100%" }}
          value={sysPrompt}
          onChange={(event) => {
            setPrompt(event.target.value);
          }}
          onKeyDown={(event) => {
            if (event.key === "Enter") {
              onPromptChange({ role: "system", content: sysPrompt });
              setSaved(true);
            }
          }}
        />
        <div className={styles.wrapper} style={{ padding: "15px" }}>
          <Button
            icon={<Save24Regular />}
            iconPosition="after"
            onClick={() => {
              onPromptChange({ role: "system", content: sysPrompt });
              setSaved(true);
            }}
          >
            Save Changes
          </Button>
          {isSaved && (
            <Body1
              style={{
                color: "GrayText",
                transition: "opacity 1s",
                opacity: 1,
                textAlign: "center",
              }}
            >
              System Message updated
            </Body1>
          )}
        </div>
      </div>
      <div>
        <Divider></Divider>
      </div>
    </Card>
  );
};
