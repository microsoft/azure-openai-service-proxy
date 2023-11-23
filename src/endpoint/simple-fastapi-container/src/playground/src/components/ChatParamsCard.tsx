import { Label, Select } from "@fluentui/react-components";
import { ParamInput } from "./ParamInput";
import { useCallback } from "react";
import { UsageData } from "../interfaces/UsageData";
import { useEventDataContext } from "../providers/EventDataProvider";
import { DividerBlock } from "./DividerBlock";
import type {
  FunctionDefinition,
  GetChatCompletionsOptions,
} from "@azure/openai";
import { Card } from "./Card";
import { ParamInputLabel } from "./ParamInputLabel";

interface ChatParamsCardProps {
  startValues: GetChatCompletionsOptions;
  tokenUpdate: (
    label: keyof GetChatCompletionsOptions,
    newValue: number | string
  ) => void;
  usageData: UsageData;
  functions: FunctionDefinition[] | undefined;
}

export const ChatParamsCard = ({
  startValues,
  tokenUpdate,
  usageData,
  functions,
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
  const maxTokens = eventData?.max_token_cap ?? 0;

  return (
    <Card header="Configuration">
      <DividerBlock>
        <ParamInput
          label="Tokens"
          defaultValue={maxTokens / 2}
          onUpdate={updateParams("maxTokens")}
          type="number"
          min={1}
          max={maxTokens}
          disabled={!isAuthorized}
        />
      </DividerBlock>

      <DividerBlock>
        <ParamInput
          label="Temperature"
          defaultValue={startValues.temperature || 0}
          onUpdate={updateParams("temperature")}
          type="number"
          min={0}
          max={1}
          disabled={!isAuthorized}
        />
      </DividerBlock>

      <DividerBlock>
        <ParamInput
          label="Top P"
          defaultValue={startValues.topP || 0}
          onUpdate={updateParams("topP")}
          type="number"
          min={0}
          max={1}
          disabled={!isAuthorized}
        />
      </DividerBlock>

      <DividerBlock>
        <>
          <ParamInputLabel label="OpenAI Functions" id="functions" />
          <Select
            id="functions"
            disabled={
              !isAuthorized ||
              (functions !== undefined && functions.length === 0)
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
                functions.map((f) => (
                  <option key={f.name} value={f.name}>
                    {f.name}
                  </option>
                ))}
            </optgroup>
          </Select>
        </>
      </DividerBlock>

      <DividerBlock>
        <Label
          style={{ color: "GrayText", fontSize: "small", textAlign: "justify" }}
        >
          <div>Finish Reason: {usageData.finish_reason}</div>
          <div>Completion Tokens: {usageData.completion_tokens}</div>
          <div>Prompt Tokens: {usageData.prompt_tokens}</div>
          <div>Total Tokens: {usageData.total_tokens}</div>
          <div>Response Time: {usageData.response_time} ms</div>
        </Label>
      </DividerBlock>
    </Card>
  );
};
