import { Select, makeStyles, Label, Tooltip } from "@fluentui/react-components";
import { Card } from "./Card";
// import { DividerBlock } from "./DividerBlock";
// import { ParamInput } from "./ParamInput";
import { GetImagesOptions } from "@azure/openai";
import { useEventDataContext } from "../../providers/EventDataProvider";
// import { ParamInputLabel } from "./ParamInputLabel";
import { Info16Filled } from "@fluentui/react-icons";

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
    marginBottom: "0px",
    marginTop: "0px",
    textAlign: "justify",
    display: "block",
    fontWeight: "bold",
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

type ImageParamsCardProps = {
  updateSettings: (
    label: keyof GetImagesOptions | "model",
    newValue: number | string
  ) => void;

  settings: GetImagesOptions;
};

export const ImageParamsCard = ({
  updateSettings,
  // settings,
}: ImageParamsCardProps) => {
  // const selectId = useId("size");
  const { eventData, isAuthorized } = useEventDataContext();
  const styles = useStyles();

  return (
    <div className={styles.body}>
      <Card header="Configuration">

          <Label className={styles.label} htmlFor="ModelLabel" style={{ marginBottom: "0px", paddingBottom: "0px" }}>
            Model
            <Tooltip content="Select the model to use for the AI chat. The model determines the type of responses the AI will generate. Different models have different capabilities and are trained on different types of data."
              relationship="description" >
              <Info16Filled className={styles.tooltip} />
            </Tooltip>
          </Label>

          <Select
            id="capabilities"
            style={{ marginTop: "0px", marginBottom: "0px" }}
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

      </Card>
    </div>
  );
};
