import {
  Body1,
  Card,
  CardHeader,
  Label,
  makeStyles,
} from "@fluentui/react-components";
import { ParamInput } from "./ParamInput";
import { useCallback } from "react";
import { UsageData } from "../interfaces/UsageData";
import { useEventDataContext } from "../providers/EventDataProvider";
import { DividerBlock } from "./DividerBlock";
import { EventCodeInput } from "./EventCodeInput";
import type { GetChatCompletionsOptions } from "@azure/openai";

const useStyles = makeStyles({
  card: {
    marginTop: "10px",
    marginRight: "10px",
    marginBottom: "10px",
    marginLeft: "10px",
  },
  dividerline: {
    maxHeight: "1%",
  },
});

interface ParamsCardProps {
  startValues: GetChatCompletionsOptions;
  tokenUpdate: (
    label: keyof GetChatCompletionsOptions,
    newValue: number | string
  ) => void;
  usageData: UsageData;
}

export const ParamsCard = ({
  startValues,
  tokenUpdate,
  usageData,
}: ParamsCardProps) => {
  const styles = useStyles();
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
    <Card className={styles.card}>
      <CardHeader
        style={{ height: "10vh", alignItems: "start" }}
        header={
          <Body1 style={{ fontSize: "large" }}>
            <h2>Configuration</h2>
          </Body1>
        }
      />

      <EventCodeInput />

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
