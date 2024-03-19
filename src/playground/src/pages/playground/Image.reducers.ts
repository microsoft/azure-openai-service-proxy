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
      payload: string;
    }
  | {
      type: "imageComplete";
      payload: ImageGenerations;
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
        prompt: action.payload,
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
  payload: ImageGenerations
): ImageState {
  console.log(payload);
  return { ...state, isLoading: false, images: [...state.images, payload] };
}
