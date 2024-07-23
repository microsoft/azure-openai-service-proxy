import {
  Body1,
  Button,
  Textarea,
  makeStyles,
  shorthands
} from "@fluentui/react-components";
import { Dispatch, useEffect, useState } from "react";
import { Save24Regular, ArrowReset24Regular } from "@fluentui/react-icons";
import type {
  ChatRequestSystemMessage,
  FunctionDefinition,
} from "@azure/openai";
import { Card } from "./Card";
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { solarizedlight } from "react-syntax-highlighter/dist/esm/styles/prism";

interface SystemProps {
  defaultPrompt: ChatRequestSystemMessage;
  systemPromptChange: Dispatch<string>;
  functionsChange: Dispatch<FunctionDefinition[]>;
}

const useStyles = makeStyles({
  wrapper: {
    width: "100%",
    marginBottom: "12px",
    ...shorthands.padding("15px"),
  },
  body: {
    ...shorthands.padding("0px", "15px"),
    ...shorthands.margin("0px"),
  }
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
    <div className={styles.body}>
      <Card header="System Message">

        <Textarea
          style={{ width: "100%", marginBottom: "6px" }}
          value={sysPrompt}
          textarea={{ rows: 8 }}
          resize="vertical"
          onChange={(event) => setPrompt(event.currentTarget.value)}
          onKeyDown={(event) => {
            if (event.key === "Enter" && !event.shiftKey) {
              systemPromptChange(sysPrompt);
              setSaved(true);
            }
          }}
        />

        <div style={{ textAlign: "center", margin: "0px" }}>
          <Button
            icon={<Save24Regular />}
            iconPosition="before"
            appearance="primary"
            style={{ margin: "6px" }}
            onClick={() => {
              systemPromptChange(sysPrompt);
              setSaved(true);
            }}
          >
            Update system message
          </Button>

          <Button
            icon={<ArrowReset24Regular />}
            iconPosition="before"
            appearance="secondary"
            style={{ margin: "6px" }}
            onClick={() => {
              setPrompt("You are an AI assistant that helps people find information.");
              systemPromptChange("You are an AI assistant that helps people find information.");
              setSaved(true);
            }}
          >
            Reset to default
          </Button>

          {isSaved && (
            <div style={{ paddingTop: "12px" }}>
              <Body1
                style={{
                  color: "#333",
                  transition: "opacity 1s",
                  opacity: 1,
                  textAlign: "center",
                }}
              >
                <strong>System message updated</strong>
              </Body1>
            </div>
          )}
        </div>
      </Card>

      <Card header="OpenAI Functions">
        {!editFunctions && (
          <div onClick={() => setEditFunctions(true)}>
            <SyntaxHighlighter
              language="json"
              style={solarizedlight}
              wrapLines={true}
              lineProps={{ style: { whiteSpace: 'pre-wrap' } }}
            >
              {functions === ""
                ? ""
                : JSON.stringify(JSON.parse(functions), null, 2)}
            </SyntaxHighlighter>
          </div>
        )}
        {editFunctions && (
          <div>
            <Textarea
              style={{ width: "100%", marginBottom: "24px" }}
              resize="vertical"
              textarea={{ rows: 10, style: { maxHeight: "fit-content" } }}
              value={functions || ``}
              onChange={(_, data) => {
                setFunctions(data.value);
              }}
            />

            <div style={{ marginBottom: "0px", textAlign: "center", padding: "" }}>
              <Button
                icon={<Save24Regular />}
                iconPosition="before"
                appearance="primary"
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
      </Card>
    </div>
  );
};
