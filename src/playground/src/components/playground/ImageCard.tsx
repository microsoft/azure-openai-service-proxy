import {
  // Body1,
  Button,
  Input,
  Label,
  Textarea,
  makeStyles,
  shorthands,
  useId,
  Tooltip
} from "@fluentui/react-components";
import { useEventDataContext } from "../../providers/EventDataProvider";
import { Card } from "./Card";
import { Dispatch, useState } from "react";
import { ExtendedImageGenerations } from "../../pages/playground/Image.state";
import { Info16Filled, SendRegular } from "@fluentui/react-icons";


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
  tooltip: {
    marginLeft: "6px",
  },
});

const ImagePrompt = ({ generateImage, isGenerating, setGenerating }: { generateImage: Dispatch<string>, isGenerating: boolean, setGenerating: Dispatch<React.SetStateAction<boolean>> }) => {
  const styles = useStyles();
  const promptId = useId();
  const [prompt, setPrompt] = useState("");
  // const { isAuthorized } = useEventDataContext();
  return (
    <div className={styles.searchRoot}>

      <Label
        style={{ fontSize: "medium", marginBottom: "0.5rem" }}
        htmlFor={promptId}
      >
        <strong>Prompt</strong>
        <Tooltip content="Describe the image you want to create. For example, 'watercolor painting of the Seattle skyline'" relationship="description" >
          <Info16Filled className={styles.tooltip} />
        </Tooltip>
      </Label>

      <Textarea
        style={{ width: "100%", maxWidth: "800px" }}
        id={promptId}
        value={prompt}
        disabled={isGenerating}
        placeholder="Enter a prompt. Eg: cute picture of an ca (Shift + Enter for new line)"
        onChange={(e) => {
          setPrompt(e.currentTarget.value);
        }}
      />
      <div>
        <Button
          onClick={() => {
            // generateImage(prompt)
            // setPrompt("");
            if(!isGenerating) {
              generateImage(prompt)
              setPrompt("");
              setGenerating(true);
            }
          }}
          disabled={!prompt || isGenerating}
          icon={<SendRegular />}
          appearance="primary"
          style={{ textAlign: "left", marginBottom: "12px", marginTop: "12px" }}
        >
          Generate
        </Button>
      </div>
    </div>
  );
};

const ImageList = ({ images, isGenerating, setGenerating }: { images: ExtendedImageGenerations[], isGenerating: boolean, setGenerating: Dispatch<React.SetStateAction<boolean>> }) => {
  // const styles = useStyles();
  return (

    <div style={{
      border: "0px solid #000",
      flexDirection: "row-reverse",
      display: "flex",
      alignItems: "",
      justifyContent: "flex-end",
      flexWrap: "wrap-reverse"
    }}>

      {images.map((image) => (

        <div
          key={image.id}
          style={{
            padding: "12px",
            borderRadius: "5px",
            width: "100%",
            maxWidth: "300px",
            display: "flex",
            boxShadow:
              "0px 0px 4px rgba(0, 0, 0, 0.36), 0px 0px 2px rgba(0, 0, 0, 0.24)",
            marginRight: "24px",
            marginBottom: "24px",
            flex: "1 0 30%",
          }}
        >

          {!image.loaded && <p>Processing...</p>}
          {image.generations &&
            image.generations.data.map((i) => {
              const url = i.url;

              if(isGenerating) {
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
                        style={{ cursor: "pointer", width: "100%", height: "100%", marginBottom: "12px" }}
                      />
                    </div>
                    <div style={{ float: "left", textAlign: "left" }}>
                      <strong>Original Prompt</strong>
                      <p>
                        {image.prompt}
                      </p>

                      <strong>Revised prompt</strong>
                      {/* <p>
                        {i.revised_prompt}
                      </p> */}
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
}: {
  generateImage: Dispatch<string>;
  images: ExtendedImageGenerations[];
}) => {
  const styles = useStyles();
  const [isGenerating, setGenerating] = useState(false);
  return (
    <div className={styles.body}>
      <Card header="DALL·E playground">

        <ImagePrompt generateImage={generateImage} isGenerating={isGenerating} setGenerating={setGenerating} />
        <ImageList images={images} isGenerating={isGenerating} setGenerating={setGenerating} />

      </Card>
    </div>
  );
};
