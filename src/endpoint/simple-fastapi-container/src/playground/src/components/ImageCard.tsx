import {
  Body1,
  Button,
  Dialog,
  DialogActions,
  DialogBody,
  DialogContent,
  DialogSurface,
  DialogTitle,
  DialogTrigger,
  Input,
  Label,
  Spinner,
  makeStyles,
  shorthands,
  useId,
} from "@fluentui/react-components";
import { useEventDataContext } from "../providers/EventDataProvider";
import { Card } from "./Card";
import { Dispatch, useState } from "react";
import { ImageGenerations, ImageLocation } from "@azure/openai";

export type ImageDetails = {
  id: number;
  prompt: string;
  loaded: boolean;
  generation?: ImageGenerations;
};

const useStyles = makeStyles({
  startCard: {
    display: "flex",
    maxWidth: "80%",
    marginTop: "35%",
    marginLeft: "20%",
    marginRight: "20%",
    marginBottom: "35%",
  },

  searchRoot: {
    display: "flex",
    flexDirection: "column",
    ...shorthands.gap("2px"),
    maxWidth: "100%",
    textAlign: "left",
  },

  imageList: {
    display: "flex",
    ...shorthands.gap("2px"),
    width: "200px",
    height: "200px",
  },
});

const Search = ({ generateImage }: { generateImage: Dispatch<string> }) => {
  const styles = useStyles();
  const promptId = useId();
  const [prompt, setPrompt] = useState("");
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
        onChange={(e) => setPrompt(e.target.value)}
        placeholder="Enter a prompt. Eg: cute picture of an cat"
      />
      <Button onClick={() => generateImage(prompt)} disabled={!prompt}>
        Generate
      </Button>
    </div>
  );
};

const ImageList = ({ images }: { images: ImageDetails[] }) => {
  const styles = useStyles();
  return (
    <>
      {images.map((image) => (
        <div
          key={image.id}
          style={{
            border: "1px solid #ccc",
            padding: "3px",
            flexGrow: "unset",
            borderRadius: "5px",
          }}
        >
          <div className={styles.imageList}>
            {image.loaded &&
              image.generation?.data.map((i) => {
                const url = (i as ImageLocation).url;
                return (
                  <img
                    src={url}
                    key={url}
                    onClick={() => window.open(url)}
                    style={{ cursor: "pointer" }}
                  />
                );
              })}
          </div>
          {!image.loaded && <Spinner />}
          <p>{image.prompt}</p>
        </div>
      ))}
    </>
  );
};

export const ImageCard = ({
  generateImage,
  images,
}: {
  generateImage: Dispatch<string>;
  images: ImageDetails[];
}) => {
  const { isAuthorized } = useEventDataContext();
  const styles = useStyles();
  return (
    <Card header="DALL·E playground">
      {!isAuthorized && (
        <Card className={styles.startCard}>
          <Body1 style={{ textAlign: "center" }}>
            <h2>Sign in to generate images.</h2>
          </Body1>
        </Card>
      )}

      {isAuthorized && <Search generateImage={generateImage} />}
      {isAuthorized && <ImageList images={images} />}
    </Card>
  );
};
