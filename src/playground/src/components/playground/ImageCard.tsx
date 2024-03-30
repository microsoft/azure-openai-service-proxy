import {
  Body1,
  Button,
  Input,
  Label,
  makeStyles,
  shorthands,
  useId,
} from "@fluentui/react-components";
import { useEventDataContext } from "../../providers/EventDataProvider";
import { Card } from "./Card";
import { Dispatch, useState } from "react";
import { ExtendedImageGenerations } from "../../pages/playground/Image.state";

const useStyles = makeStyles({
  startCard: {
    display: "flex",
    maxWidth: "80%",
    marginTop: "35%",
    marginLeft: "20%",
    marginRight: "20%",
    marginBottom: "35%",
  },

  body: {
    paddingLeft: "15px",
    paddingRight: "15px",
    marginTop: "0px",
    marginRight: "0px",
    marginBottom: "0px",
    marginLeft: "0px",
  },

  searchRoot: {
    display: "flex",
    flexDirection: "column",
    ...shorthands.gap("2px"),
    maxWidth: "100%",
    textAlign: "left",
  },

  container: {
    display: "grid",
    gridTemplateRows: "1fr 1fr 5fr",
  },

  imageList: {
    ...shorthands.border("1px", "solid", "#ccc"),
    display: "flex",
  },

  image: {
    display: "flex",
    flexDirection: "column",
    ...shorthands.padding("15px"),
    ...shorthands.margin("10px"),
    ...shorthands.border("1px", "solid", "#333"),
    maxHeight: "320px",
  },

  imageContainer: {
    display: "flex",
    ...shorthands.gap("2px"),
    width: "300px",
    height: "300px",
    flexDirection: "column",
    ...shorthands.overflow("hidden"),

    "& img": {
      width: "100%",
      height: "100%",
      objectFit: "cover",
    },
  },
});

const ImagePrompt = ({
  generateImage,
  canGenerate,
}: {
  generateImage: Dispatch<string>;
  canGenerate: boolean;
}) => {
  const styles = useStyles();
  const promptId = useId();
  const [prompt, setPrompt] = useState("");
  const { isAuthorized } = useEventDataContext();
  return (
    <div className={styles.searchRoot}>
      <Label
        style={{ fontSize: "medium", marginBottom: "0.5rem" }}
        htmlFor={promptId}
      >
        <strong>Prompt</strong>
      </Label>
      <Input
        type="text"
        size="medium"
        id={promptId}
        value={prompt}
        disabled={!isAuthorized}
        onChange={(e) => setPrompt(e.target.value)}
        placeholder="Enter a prompt. Eg: cute picture of an cat"
      />
      <Button
        onClick={() => generateImage(prompt)}
        disabled={!prompt || !canGenerate}
      >
        Generate
      </Button>
    </div>
  );
};

const ImageList = ({ images }: { images: ExtendedImageGenerations[] }) => {
  const styles = useStyles();
  return (
    <div className={styles.imageList}>
      {images.map((image) => (
        <div key={image.id} className={styles.image}>
          <div className={styles.imageContainer}>
            {!image.loaded && <p>Processing...</p>}
            {image.generations &&
              image.generations.data.map((i) => {
                const url = i.url;
                return (
                  <>
                    <img
                      src={url}
                      key={url}
                      onClick={() => window.open(url)}
                      style={{ cursor: "pointer" }}
                    />
                  </>
                );
              })}
            {image.isError && <p>Error: {image.errorInfo?.message}</p>}
          </div>
          <p>{image.prompt}</p>
        </div>
      ))}
    </div>
  );
};

export const ImageCard = ({
  generateImage,
  images,
  canGenerate,
}: {
  generateImage: Dispatch<string>;
  images: ExtendedImageGenerations[];
  canGenerate: boolean;
}) => {
  const styles = useStyles();
  return (
    <div className={styles.body}>
      <Card header="DALL·E playground">

        <ImagePrompt generateImage={generateImage} canGenerate={canGenerate} />
        <ImageList images={images} />

      </Card>
    </div>
  );
};
