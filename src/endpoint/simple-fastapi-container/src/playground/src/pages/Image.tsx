import { makeStyles } from "@fluentui/react-components";
import { ImageParamsCard } from "../components/ImageParamsCard";
import { ImageGenerationOptions, ImageGenerations } from "@azure/openai";
import { useState } from "react";
import { useEventDataContext } from "../providers/EventDataProvider";
import { ImageCard, ImageDetails } from "../components/ImageCard";
import { useOpenAIClientContext } from "../providers/OpenAIProvider";

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
  const { client } = useOpenAIClientContext();

  const [imageSettings, setImageSettings] = useState<ImageGenerationOptions>({
    n: 1,
    size: "512x512",
    responseFormat: "url",
    user: eventCode,
  });

  const [images, setImages] = useState<ImageDetails[]>([]);

  const updateSettings = (
    label: keyof ImageGenerationOptions,
    newValue: number | string
  ) => {
    setImageSettings({
      ...imageSettings,
      [label]: newValue,
    });
  };

  const generateImage = async (prompt: string) => {
    if (!client) {
      return;
    }

    const id = Date.now()

    setImages((current) => [...current, { prompt, loaded: false, id }]);

    const response = await client.getImages(prompt, imageSettings);

    setImages((current) => {
      const updated = [...current];
      const index = updated.findIndex((image) => image.id === id);
      updated[index] = {
        ...updated[index],
        loaded: true,
        generation: response,
      };
      return updated;
    });
  };

  return (
    <section className={styles.container}>
      <ImageCard generateImage={generateImage} images={images} />
      <ImageParamsCard
        updateSettings={updateSettings}
        settings={imageSettings}
      />
    </section>
  );
};
