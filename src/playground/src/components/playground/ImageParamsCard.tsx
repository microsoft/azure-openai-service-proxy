import { makeStyles, shorthands } from "@fluentui/react-components";
import { Card } from "./Card";
import { GetImagesOptions } from "@azure/openai";
import { useEventDataContext } from "../../providers/EventDataProvider";
import { ParamSelect } from "./controls/ParamSelect";

const useStyles = makeStyles({
  container: {
    ...shorthands.margin("0px", "0px", "24px"),
  },

  body: {
    ...shorthands.padding("0px", "15px"),
    ...shorthands.margin("0px"),
  },
});

type ImageParamsCardProps = {
  updateSettings: (
    label: keyof GetImagesOptions | "model",
    newValue: number | string
  ) => void;
  settings: GetImagesOptions;
};

export const ImageParamsCard = ({ updateSettings }: ImageParamsCardProps) => {
  const { eventData, isAuthorized } = useEventDataContext();
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
              eventData.capabilities["openai-dalle3"] &&
              eventData.capabilities["openai-dalle3"]) ||
            []
          }
          onUpdate={(newValue) => updateSettings("model", newValue)}
        />
      </Card>
    </div>
  );
};
