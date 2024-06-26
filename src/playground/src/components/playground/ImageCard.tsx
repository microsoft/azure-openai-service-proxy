import {
  Button,
  Select,
  Label,
  Textarea,
  makeStyles,
  shorthands,
  useId,
  Tooltip,
} from "@fluentui/react-components";
import { Card } from "./Card";
import { Dispatch, useState } from "react";
import { ExtendedImageGenerations } from "../../pages/playground/Image.state";
import {
  Info16Filled,
  SendRegular,
  Delete24Regular,
} from "@fluentui/react-icons";
import { useEventDataContext } from "../../providers/EventDataProvider";
import { GetImagesOptions } from "@azure/openai";

const useStyles = makeStyles({
  body: {
    ...shorthands.padding("0px", "15px"),
    ...shorthands.margin("0px"),
  },

  searchRoot: {
    display: "flex",
    flexDirection: "column",
    ...shorthands.gap("2px"),
    maxWidth: "100%",
    textAlign: "left",
  },

  label: {
    fontSize: "medium",
    ...shorthands.margin("0px", "0"),
    textAlign: "justify",
    display: "block",
    fontWeight: "bold",
  },

  tooltip: {
    marginLeft: "6px",
  },

  smallButton: {
    ...shorthands.margin("12px", "12px", "12px"),
  },

  modelSelect: {
    ...shorthands.margin("0px", "0px"),
    maxWidth: "200px",
  },

  imageContainer: {
    ...shorthands.border("0px", "solid", "#000"),
    flexDirection: "row-reverse",
    display: "flex",
    alignItems: "",
    justifyContent: "flex-end",
    flexWrap: "wrap-reverse",
  },

  imageItem: {
    ...shorthands.padding("12px"),
    ...shorthands.borderRadius("5px"),
    width: "100%",
    maxWidth: "300px",
    display: "flex",
    boxShadow:
      "0px 0px 4px rgba(0, 0, 0, 0.36), 0px 0px 2px rgba(0, 0, 0, 0.24)",
    marginRight: "24px",
    marginBottom: "24px",
    ...shorthands.flex("1", "0", "30%"),
  },
});

const ImagePrompt = ({
  generateImage,
  isGenerating,
  setGenerating,
  updateSettings,
}: {
  generateImage: Dispatch<string>;
  isGenerating: boolean;
  setGenerating: Dispatch<React.SetStateAction<boolean>>;
  updateSettings: (
    label: keyof GetImagesOptions | "model",
    newValue: number | string
  ) => void;
}) => {
  const styles = useStyles();
  const promptId = useId();
  const [prompt, setPrompt] = useState("");
  const { eventData, isAuthorized } = useEventDataContext();

  return (
    <div className={styles.searchRoot}>
      <Label
        className={styles.label}
        htmlFor="ModelLabel"
        style={{ marginBottom: "0px", paddingBottom: "0px" }}
      >
        Model
        <Tooltip
          content="Select the model to use for the AI chat. The model determines the type of responses the AI will generate. Different models have different capabilities and are trained on different types of data."
          relationship="description"
        >
          <Info16Filled className={styles.tooltip} />
        </Tooltip>
      </Label>

      <Select
        id="capabilities"
        className={styles.modelSelect}
        disabled={!isAuthorized}
        onChange={(e) => {
          const newValue = e.currentTarget.value;
          updateSettings("model", newValue);
          // check if the model is selected
          if (newValue) {
            setGenerating(false);
          } else {
            setGenerating(true);
          }
          setPrompt("");
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

      <Label
        style={{ fontSize: "medium", marginBottom: "0.5rem" }}
        htmlFor={promptId}
      >
        <strong>Prompt</strong>
        <Tooltip
          content="Describe the image you want to create. For example, 'watercolor painting of the Seattle skyline'"
          relationship="description"
        >
          <Info16Filled className={styles.tooltip} />
        </Tooltip>
      </Label>

      <Textarea
        style={{ width: "90%" }}
        id={promptId}
        value={prompt}
        disabled={isGenerating}
        placeholder="Enter a prompt. Eg: cute picture of an cat (Shift + Enter for new line)"
        onChange={(e) => {
          setPrompt(e.currentTarget.value);
        }}
        onKeyDown={(e) => {
          if (e.key === "Enter" && !e.shiftKey && prompt) {
            generateImage(prompt);
            setGenerating(true);
            e.preventDefault();
          }
        }}
      />
      <div>
        <Button
          onClick={() => {
            if (!isGenerating) {
              generateImage(prompt);
              setGenerating(true);
            }
          }}
          disabled={!prompt || isGenerating}
          className={styles.smallButton}
          icon={<SendRegular />}
          appearance="primary"
          style={{ textAlign: "left", marginBottom: "12px", marginTop: "12px" }}
        >
          Generate
        </Button>
        <Button
          onClick={() => {
            if (!isGenerating) {
              setPrompt("");
            }
          }}
          disabled={!prompt || isGenerating}
          className={styles.smallButton}
          id="clear-button"
          icon={<Delete24Regular />}
          iconPosition="before"
        >
          Clear prompt
        </Button>
      </div>
    </div>
  );
};

type ImageListProps = {
  images: ExtendedImageGenerations[];
  isGenerating: boolean;
  setGenerating: Dispatch<React.SetStateAction<boolean>>;
};

const ImageList = ({ images, isGenerating, setGenerating }: ImageListProps) => {
  const styles = useStyles();
  return (
    <div className={styles.imageContainer}>
      {images.map((image) => (
        <div key={image.id} className={styles.imageItem}>
          {!image.loaded && <p>Processing...</p>}
          {image.generations &&
            image.generations.data.map((i) => {
              const url = i.url;
              const revisedPrompt = i.revisedPrompt;

              if (isGenerating) {
                setGenerating(false);
              }

              return (
                <>
                  <div key={image.id}>
                    <div>
                      <img
                        src={url}
                        key={url}
                        onClick={() => window.open(url)}
                        style={{
                          cursor: "pointer",
                          width: "100%",
                          height: "100%",
                          marginBottom: "12px",
                        }}
                      />
                    </div>
                    <div style={{ float: "left", textAlign: "left" }}>
                      <strong>Original Prompt</strong>
                      <p>{image.prompt}</p>

                      <strong>Revised prompt</strong>
                      <p>{revisedPrompt}</p>
                    </div>
                  </div>
                </>
              );
            })}
          {image.isError && <p>Error: {image.errorInfo?.message}</p>}
        </div>
      ))}
    </div>
  );
};

export const ImageCard = ({
  generateImage,
  images,
  updateSettings,
}: {
  generateImage: Dispatch<string>;
  images: ExtendedImageGenerations[];
  updateSettings: (
    label: keyof GetImagesOptions | "model",
    newValue: number | string
  ) => void;
}) => {
  const styles = useStyles();
  const [isGenerating, setGenerating] = useState(true);
  const { isAuthorized } = useEventDataContext();

  return (
    <div className={styles.body}>
      <Card header="DALL·E playground">
        {isAuthorized && (
          <>
            <ImagePrompt
              generateImage={generateImage}
              isGenerating={isGenerating}
              setGenerating={setGenerating}
              updateSettings={updateSettings}
            />
            <ImageList
              images={images}
              isGenerating={isGenerating}
              setGenerating={setGenerating}
            />
          </>
        )}
      </Card>
    </div>
  );
};
