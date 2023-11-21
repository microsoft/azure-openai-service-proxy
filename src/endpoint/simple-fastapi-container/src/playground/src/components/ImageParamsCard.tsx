import { Label, Select, useId } from "@fluentui/react-components";
import { Card } from "./Card";
import { EventCodeInput } from "./EventCodeInput";
import { DividerBlock } from "./DividerBlock";
import { ParamInput } from "./ParamInput";
import { ImageGenerationOptions } from "@azure/openai";
import { useEventDataContext } from "../providers/EventDataProvider";

type ImageParamsCardProps = {
  updateSettings: (
    label: keyof ImageGenerationOptions,
    newValue: number | string
  ) => void;

  settings: ImageGenerationOptions;
};

export const ImageParamsCard = ({
  updateSettings,
  settings,
}: ImageParamsCardProps) => {
  const selectId = useId("size");
  const { isAuthorized } = useEventDataContext();
  return (
    <Card header="Configuration">
      <EventCodeInput />

      <DividerBlock>
        <ParamInput
          label="Number of images"
          type="number"
          min={1}
          max={10}
          defaultValue={settings.n || 1}
          onUpdate={(value) => updateSettings("n", value)}
          disabled={!isAuthorized}
        />
      </DividerBlock>

      <DividerBlock>
        <Label
          style={{
            fontSize: "medium",
            marginBottom: "0.5rem",
            textAlign: "justify",
          }}
          htmlFor={selectId}
        >
          <strong>Image size</strong>
        </Label>
        <Select
          onChange={(_, data) => updateSettings("size", data.value)}
          id={selectId}
          value={settings.size}
          disabled={!isAuthorized}
        >
          <option value="256x256">256x256</option>
          <option value="512x512">512x512</option>
          <option value="1024x1024">1024x1024</option>
        </Select>
      </DividerBlock>
    </Card>
  );
};
