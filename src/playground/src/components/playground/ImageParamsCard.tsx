import { Label, Select, useId } from "@fluentui/react-components";
import { Card } from "./Card";
import { DividerBlock } from "./DividerBlock";
import { ParamInput } from "./ParamInput";
import { GetImagesOptions } from "@azure/openai";
import { useEventDataContext } from "../../providers/EventDataProvider";
import { ParamInputLabel } from "./ParamInputLabel";

type ImageParamsCardProps = {
  updateSettings: (
    label: keyof GetImagesOptions | "model",
    newValue: number | string
  ) => void;

  settings: GetImagesOptions;
};

export const ImageParamsCard = ({
  updateSettings,
  settings,
}: ImageParamsCardProps) => {
  const selectId = useId("size");
  const { eventData, isAuthorized } = useEventDataContext();
  return (
    <Card header="Configuration">
      <DividerBlock>
      <>
          <ParamInputLabel label="Model deployment" id="capabilities" />
          <Select
            id="capabilities"
            disabled={!isAuthorized}
            onChange={(e) => {
              const newValue = e.currentTarget.value;
              updateSettings("model", newValue);
            }}
          >
            <option value="">Select a model</option>
            {eventData &&
              eventData.capabilities["openai-dalle3"] &&
              eventData.capabilities["openai-dalle3"].map((model) => (
                <option key={model} value={model}>
                  {model}
                </option>
              ))}
          </Select>
        </>
      </DividerBlock>

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
          <option value="1024x1024">1024x1024</option>
          <option value="1792x1024">1792x1024</option>
          <option value="1024x1792">1792</option>
        </Select>
      </DividerBlock>
    </Card>
  );
};
