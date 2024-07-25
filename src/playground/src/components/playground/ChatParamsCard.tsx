import { Select, makeStyles, shorthands } from "@fluentui/react-components";
import { ParamInput } from "./controls/ParamInput";
import { useCallback } from "react";
import { UsageData } from "../../interfaces/UsageData";
import { useEventDataContext } from "../../providers/EventDataProvider";
import type { GetChatCompletionsOptions } from "@azure/openai";
import { Card } from "./Card";
import { ParamSelect } from "./controls/ParamSelect";
import { LabelWithTooltip } from "./controls/LabelWithTooltip";

interface ChatParamsCardProps {
  startValues: GetChatCompletionsOptions;
  tokenUpdate: (
    label: keyof GetChatCompletionsOptions | "model",
    newValue: number | string
  ) => void;
  usageData: UsageData;
}

const useStyles = makeStyles({
  container: {
    ...shorthands.margin("0px", "0px", "24px"),
  },
  body: {
    ...shorthands.padding("0px", "15px"),
    ...shorthands.margin("0px"),
  },
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
        <ParamSelect
          label="Model"
          explain="Select the model to use for the AI chat. The model determines the type of responses the AI will generate. Different models have different capabilities and are trained on different types of data."
          disabled={!isAuthorized}
          defaultOption="Select a model"
          options={
            (eventData &&
              eventData.capabilities["openai-chat"] &&
              eventData.capabilities["openai-chat"]) ||
            []
          }
          onUpdate={(newValue) => tokenUpdate("model", newValue)}
        />

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

        <ParamSelect
          label="Temperature"
          explain="Controls randomness. Lowering the temperature means that the model will produce more repetitive and deterministic responses. Increasing the temperature will result in more unexpected or creative responses. Try adjusting temperature or Top P but not both."
          onUpdate={(newValue) => tokenUpdate("temperature", parseFloat(newValue))}
          disabled={!isAuthorized}
          options={[
            "0",
            "0.1",
            "0.2",
            "0.3",
            "0.4",
            "0.5",
            "0.6",
            "0.7",
            "0.8",
            "0.9",
            "1",
          ]}
          defaultValue={startValues.temperature || "0"}
        />

        <ParamSelect
          label="Top P"
          explain="Similar to temperature, this controls randomness but uses a different method. Lowering Top P will narrow the model's token selection to likelier tokens. Increasing Top P will let the model choose from tokens with both high and low likelihood. Try adjusting temperature or Top P but not both."
          onUpdate={(newValue) => tokenUpdate("topP", parseFloat(newValue))}
          disabled={!isAuthorized}
          options={[
            "0",
            "0.1",
            "0.2",
            "0.3",
            "0.4",
            "0.5",
            "0.6",
            "0.7",
            "0.8",
            "0.9",
            "1",
          ]}
          defaultValue={startValues.topP || "0"}
        />

        <LabelWithTooltip
          label="OpenAI Functions"
          id="functions"
          explain="OpenAI Functions are custom functions that can be used to process the AI response. They can be used to filter, modify, or enhance the AI output. Select 'auto' to use the default function or 'none' to disable functions."
        />

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
          <p
            style={{
              marginBottom: "6px",
              paddingBottom: "6px",
              borderBottom: "1px solid #306ab7",
            }}
          >
            <LabelWithTooltip
              label="Tokens"
              id="topP"
              explain="Input/output tokens in AI processing are pieces of text, like words or punctuation, that AI models like GPT-3 use to understand and generate language. They're counted to measure usage for processing and billing."
            />
          </p>
          <strong>Finish Reason:</strong> {usageData.finish_reason}
          <br />
          <strong>Completion Tokens:</strong> {usageData.completion_tokens}
          <br />
          <strong>Prompt Tokens:</strong> {usageData.prompt_tokens}
          <br />
          <strong>Total Tokens:</strong> {usageData.total_tokens}
          <br />
          <br />
          <strong>Response Time:</strong> {usageData.response_time} ms
        </div>
      </Card>
    </div>
  );
};
