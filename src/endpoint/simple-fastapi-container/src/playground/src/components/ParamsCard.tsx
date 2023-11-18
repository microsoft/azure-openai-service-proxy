import {
  Body1,
  Card,
  CardFooter,
  CardHeader,
  Label,
  makeStyles,
} from "@fluentui/react-components";
import { ApiData } from "../interfaces/ApiData";
import { ParamInput } from "./ParamInput";
import { useCallback } from "react";
import { UsageData } from "../interfaces/UsageData";
import { useEventDataContext } from "../EventDataProvider";
import { DividerBlock } from "./DividerBlock";
import { EventCodeInput } from "./EventCodeInput";

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
  startValues: Omit<ApiData, "messages">;
  tokenUpdate: (
    label: keyof Omit<ApiData, "messages">,
    newValue: number | string
  ) => void;
  name: string;
  usageData: UsageData;
}

export const ParamsCard = ({
  startValues,
  tokenUpdate,
  name,
  usageData,
}: ParamsCardProps) => {
  const styles = useStyles();
  const updateParams = useCallback(
    (label: keyof Omit<ApiData, "messages">) => {
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
          onUpdate={updateParams("max_tokens")}
          type="number"
          min={1}
          max={maxTokens}
          disabled={!isAuthorized}
        />
      </DividerBlock>

      <DividerBlock>
        <ParamInput
          label="Temperature"
          defaultValue={startValues.temperature}
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
          defaultValue={startValues.top_p}
          onUpdate={updateParams("top_p")}
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

      <CardFooter style={{ height: "5vh" }}>
        <Label
          style={{ color: "GrayText", fontSize: "small", textAlign: "center" }}
        >
          {name}
        </Label>
      </CardFooter>
    </Card>
  );
};
