import { makeStyles } from "@fluentui/react-components";
import { GetImagesOptions } from "@azure/openai";
import { useReducer } from "react";
import { ImageCard } from "../../components/playground/ImageCard";
import { useOpenAIClientContext } from "../../providers/OpenAIProvider";
import { reducer } from "./Image.reducers";
import { INITIAL_STATE } from "./Image.state";

const useStyles = makeStyles({
  container: {
    textAlign: "center",
    display: "grid",
    gridTemplateColumns: "1fr",
    gridGap: "1px",
  },
});

export const Image = () => {
  const styles = useStyles();
  const { client } = useOpenAIClientContext();
  const [state, dispatch] = useReducer(reducer, INITIAL_STATE);

  const updateSettings = (
    label: keyof GetImagesOptions | "model",
    newValue: number | string
  ) => {
    if (label === "model") {
      dispatch({ type: "updateModel", payload: newValue as string });
      return;
    }

    dispatch({
      type: "updateParameters",
      payload: { name: label, value: newValue },
    });
  };

  const generateImage = async (prompt: string) => {
    if (!client || !state.model) {
      return;
    }

    const id = Date.now().toString();

    dispatch({ type: "imageStart", payload: { prompt, id } });

    try {
      const response = await client.getImages(
        state.model,
        prompt,
        state.parameters
      );

      dispatch({
        type: "imageComplete",
        payload: { response, id },
      });
    } catch (error) {
      dispatch({
        type: "imageError",
        payload: { error, id },
      });
    }
  };

  return (
    <section className={styles.container}>
      <ImageCard
        generateImage={generateImage}
        images={state.images}
        updateSettings={updateSettings}
      />
    </section>
  );
};
