import {
  Body1,
  Button,
  Divider,
  Textarea,
  makeStyles,
} from "@fluentui/react-components";
import { Dispatch, useEffect, useState } from "react";
import { Save24Regular } from "@fluentui/react-icons";
import type {
  ChatRequestSystemMessage,
  FunctionDefinition,
} from "@azure/openai";
import { Card, CardHeader } from "./Card";
import { DividerBlock } from "./DividerBlock";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { solarizedlight } from "react-syntax-highlighter/dist/esm/styles/prism";

interface SystemProps {
  defaultPrompt: ChatRequestSystemMessage;
  systemPromptChange: Dispatch<string>;
  functionsChange: Dispatch<FunctionDefinition[]>;
}

const useStyles = makeStyles({
  wrapper: {
    display: "flex",
    flexDirection: "column",
    rowGap: "15px",
  },
});

export const SystemCard = ({
  defaultPrompt,
  systemPromptChange,
  functionsChange,
}: SystemProps) => {
  const [sysPrompt, setPrompt] = useState(defaultPrompt.content || "");
  const [isSaved, setSaved] = useState(false);
  const [functions, setFunctions] = useState<string>("");
  const [editFunctions, setEditFunctions] = useState(false);
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
      <div>
        <Textarea
          className="test"
          style={{ width: "100%" }}
          value={sysPrompt}
          textarea={{ rows: 10 }}
          resize="vertical"
          onChange={(event) => setPrompt(event.target.value)}
          onKeyDown={(event) => {
            if (event.key === "Enter" && !event.shiftKey) {
              systemPromptChange(sysPrompt);
              setSaved(true);
            }
          }}
        />
        <div className={styles.wrapper} style={{ padding: "15px" }}>
          <Button
            icon={<Save24Regular />}
            iconPosition="after"
            onClick={() => {
              systemPromptChange(sysPrompt);
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
      <DividerBlock>
        <CardHeader header="OpenAI Functions" />
        {!editFunctions && (
          <div onClick={() => setEditFunctions(true)}>
            <SyntaxHighlighter language="json" style={solarizedlight}>
              {functions === ""
                ? "[]"
                : JSON.stringify(JSON.parse(functions), null, 2)}
            </SyntaxHighlighter>
          </div>
        )}
        {editFunctions && (
          <div>
            <Textarea
              style={{ width: "100%" }}
              resize="vertical"
              textarea={{ rows: 25, style: { maxHeight: "fit-content" } }}
              value={functions || `[]`}
              onChange={(_, data) => {
                setFunctions(data.value);
              }}
            />
            <div className={styles.wrapper} style={{ padding: "15px" }}>
              <Button
                icon={<Save24Regular />}
                iconPosition="after"
                onClick={() => {
                  try {
                    const j = JSON.parse(functions);
                    if (!Array.isArray(j)) {
                      throw new Error("Functions JSON invalid");
                    }
                    functionsChange(j as FunctionDefinition[]);
                    setSaved(true);
                    setEditFunctions(false);
                  } catch (e) {
                    console.warn("Functions JSON invalid", e);
                  }
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
                  Functions updated
                </Body1>
              )}
            </div>
          </div>
        )}
      </DividerBlock>
    </Card>
  );
};
