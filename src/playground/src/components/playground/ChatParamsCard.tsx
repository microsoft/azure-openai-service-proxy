import { Select, Tooltip, makeStyles, Label } from "@fluentui/react-components";
import { ParamInput } from "./ParamInput";
import { useCallback } from "react";
import { UsageData } from "../../interfaces/UsageData";
import { useEventDataContext } from "../../providers/EventDataProvider";
import type { GetChatCompletionsOptions } from "@azure/openai";
import { Card } from "./Card";
import { ParamInputLabel } from "./ParamInputLabel";
import { Info16Filled } from "@fluentui/react-icons";

interface ChatParamsCardProps {
  startValues: GetChatCompletionsOptions;
  tokenUpdate: (
    label: keyof GetChatCompletionsOptions | "model",
    newValue: number | string
  ) => void;
  usageData: UsageData;
}

const useStyles = makeStyles({
  input: {
    fontSize: "medium",
    marginLeft: "0px",
    width: "100%",
    textAlign: "left",

    height: "auto",
  },
  container: {
    marginTop: "0px",
    marginBottom: "24px",
  },
  label: {
    fontSize: "medium",
    marginBottom: "0.5rem",
    textAlign: "justify",
    display: "block",
    fontWeight: "bold",
    marginTop: "12px",
  },
  tooltip: {
    marginLeft: "6px",
  },
  body: {
    paddingLeft: "15px",
    paddingRight: "15px",
    marginTop: "0px",
    marginRight: "0px",
    marginBottom: "0px",
    marginLeft: "0px",
  }
});

export const ChatParamsCard = ({
  startValues,
  tokenUpdate,
  usageData,
}: ChatParamsCardProps) => {
  const updateParams = useCallback(
    (label: keyof GetChatCompletionsOptions) => {
      return (newValue: number | string) => {
        tokenUpdate(label, newValue);
      };
    },
    [tokenUpdate]
  );
  const { eventData, isAuthorized } = useEventDataContext();

  const maxTokens = eventData?.maxTokenCap ?? 0;
  const functions = startValues.functions;
  const styles = useStyles();

  return (
    <div className={styles.body}>
      <Card header="Configuration">

        <ParamInputLabel label="Model" id="capabilities" />
        <Select
          id="capabilities"
          disabled={!isAuthorized}
          onChange={(e) => {
            const newValue = e.currentTarget.value;
            tokenUpdate("model", newValue);
          }}
        >
          <option value="">Select a model</option>
          {eventData &&
            eventData.capabilities["openai-chat"] &&
            eventData.capabilities["openai-chat"].map((model) => (
              <option key={model} value={model}>
                {model}
              </option>
            ))}
        </Select>

        <ParamInput
          label="Max response"
          defaultValue={maxTokens / 2}
          onUpdate={updateParams("maxTokens")}
          type="number"
          min={1}
          max={maxTokens}
          disabled={!isAuthorized}
          explain="Set a limit on the number of tokens per model response. The API supports a maximum of tokens shared between the prompt and the model's response."
        />

        <Label className={styles.label} htmlFor="Temperature" style={{ marginBottom: "0px", paddingBottom: "0px" }}>
          Temperature
          <Tooltip content="Controls randomness. Lowering the temperature means that the model will produce more repetitive and deterministic responses. Increasing the temperature will result in more unexpected or creative responses. Try adjusting temperature or Top P but not both."
            relationship="description" >
            <Info16Filled className={styles.tooltip} />
          </Tooltip>
        </Label>

        <Select
          id="temperature"
          className={styles.input}
          style={{ marginTop: "0px" }}
          onChange={(e) => {
            const newValue = e.target.value;
            if (newValue) {
              tokenUpdate("temperature", newValue);
            }
          }}
          disabled={!isAuthorized}
          defaultValue={startValues.temperature || 0}
        >
          <option value="0">0</option>
          <option value="0.1">0.1</option>
          <option value="0.2">0.2</option>
          <option value="0.3">0.3</option>
          <option value="0.4">0.4</option>
          <option value="0.5">0.5</option>
          <option value="0.6">0.6</option>
          <option value="0.7">0.7</option>
          <option value="0.8">0.8</option>
          <option value="0.9">0.9</option>
          <option value="1">1</option>
        </Select>

        <Label className={styles.label} htmlFor="topP" style={{ marginBottom: "0px", paddingBottom: "0px" }}>
          Top P
          <Tooltip content="Similar to temperature, this controls randomness but uses a different method. Lowering Top P will narrow the model’s token selection to likelier tokens. Increasing Top P will let the model choose from tokens with both high and low likelihood. Try adjusting temperature or Top P but not both."
            relationship="description" >
            <Info16Filled className={styles.tooltip} />
          </Tooltip>
        </Label>

        <Select
          id="topP"
          className={styles.input}
          style={{ marginTop: "0px" }}
          onChange={(e) => {
            const newValue = e.target.value;
            if (newValue) {
              tokenUpdate("topP", newValue);
            }
          }}
          disabled={!isAuthorized}
          defaultValue={startValues.topP || 0}
        >
          <option value="0">0</option>
          <option value="0.1">0.1</option>
          <option value="0.2">0.2</option>
          <option value="0.3">0.3</option>
          <option value="0.4">0.4</option>
          <option value="0.5">0.5</option>
          <option value="0.6">0.6</option>
          <option value="0.7">0.7</option>
          <option value="0.8">0.8</option>
          <option value="0.9">0.9</option>
          <option value="1">1</option>
        </Select>

        <Label className={styles.label} htmlFor="functions" style={{ marginBottom: "0px", paddingBottom: "0px" }}>
          OpenAI Functions
        </Label>

        <Select
          id="functions"
          disabled={
            !isAuthorized || functions === undefined || functions.length === 0
          }
          onChange={(e) => {
            const newValue = e.currentTarget.value;
            if (newValue) {
              tokenUpdate("functionCall", newValue);
            }
          }}
        >
          <optgroup label="Standard Operations">
            <option value="auto">auto</option>
            <option value="none">none</option>
          </optgroup>
          <optgroup label="Custom Functions">
            {functions &&
              functions
                .filter((f) => f.name)
                .map((f) => (
                  <option key={f.name} value={f.name}>
                    {f.name}
                  </option>
                ))}
          </optgroup>
        </Select>
      </Card>

      <Card header="API Response">

        <div style={{ padding: "0px", margin: "0px", marginTop: "0px" }}>

          <Label className={styles.label} htmlFor="topP" style={{ marginBottom: "6px", paddingBottom: "6px", borderBottom: "1px solid #306ab7" }}>
            Tokens
            <Tooltip content="Input/output tokens in AI processing are pieces of text, like words or punctuation, that AI models like GPT-3 use to understand and generate language. They're counted to measure usage for processing and billing."
              relationship="description" >
              <Info16Filled className={styles.tooltip} />
            </Tooltip>
          </Label>

          <strong>Finish Reason:</strong> {usageData.finish_reason}<br />
          <strong>Completion Tokens:</strong> {usageData.completion_tokens}<br />
          <strong>Prompt Tokens:</strong> {usageData.prompt_tokens}<br />
          <strong>Total Tokens:</strong> {usageData.total_tokens}<br />

          <br />
          <strong>Response Time:</strong> {usageData.response_time} ms

        </div>
      </Card>
    </div>
  );
};
