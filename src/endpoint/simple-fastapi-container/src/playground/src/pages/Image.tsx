import { makeStyles } from "@fluentui/react-components";
import { ImageParamsCard } from "../components/ImageParamsCard";
import { ImageGenerationOptions } from "@azure/openai";
import { useState } from "react";
import { useEventDataContext } from "../providers/EventDataProvider";

const useStyles = makeStyles({
  container: {
    textAlign: "center",
    display: "grid",
    gridTemplateColumns: "2.5fr 1fr",
    gridGap: "1px",
  },
});

export const Image = () => {
  const styles = useStyles();
  const { eventCode } = useEventDataContext();

  const [imageSettings, setImageSettings] = useState<ImageGenerationOptions>({
    n: 1,
    size: "512x512",
    responseFormat: "url",
    user: eventCode,
  });

  const updateSettings = (
    label: keyof ImageGenerationOptions,
    newValue: number | string
  ) => {
    setImageSettings({
      ...imageSettings,
      [label]: newValue,
    });
  };

  return (
    <section className={styles.container}>
      <div>
        <h1>Image</h1>
      </div>
      <ImageParamsCard
        updateSettings={updateSettings}
        settings={imageSettings}
      />
    </section>
  );
};
