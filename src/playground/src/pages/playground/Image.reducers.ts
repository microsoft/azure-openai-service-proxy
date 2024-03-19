import { GetImagesOptions, ImageGenerations } from "@azure/openai";
import { ImageState } from "./Image.state";

type ImageAction =
  | {
      type: "updateParameters";
      payload: {
        name: keyof GetImagesOptions;
        value: number | string;
      };
    }
  | {
      type: "updateModel";
      payload: string;
    }
  | {
      type: "imageStart";
      payload: { prompt: string; id: string };
    }
  | {
      type: "imageComplete";
      payload: { response: ImageGenerations; id: string };
    }
  | {
      type: "imageError";
      payload: string;
    };

export function reducer(state: ImageState, action: ImageAction): ImageState {
  switch (action.type) {
    case "updateParameters":
      return {
        ...state,
        parameters: {
          ...state.parameters,
          [action.payload.name]: action.payload.value,
        },
      };

    case "updateModel":
      return {
        ...state,
        model: action.payload,
      };

    case "imageStart":
      return {
        ...state,
        isLoading: true,
        images: [
          ...state.images,
          {
            prompt: action.payload.prompt,
            id: action.payload.id,
            loaded: false,
          },
        ],
      };

    case "imageComplete":
      return processImages(state, action.payload);

    case "imageError":
      return {
        ...state,
        isLoading: false,
      };

    default:
      return state;
  }
}
function processImages(
  state: ImageState,
  payload: { response: ImageGenerations; id: string }
): ImageState {
  let image = state.images.find((i) => i.id === payload.id);
  if (!image) {
    return state;
  }
  image = { ...image, generations: payload.response, loaded: true };
  return {
    ...state,
    isLoading: false,
    images: state.images.map((i) => (i.id === payload.id ? image : i)),
  };
}
