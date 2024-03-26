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
      payload: { error: unknown; id: string };
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
        images: [
          ...state.images,
          {
            prompt: action.payload.prompt,
            id: action.payload.id,
            loaded: false,
            isError: false,
          },
        ],
      };

    case "imageComplete":
      return processImages(state, action.payload);

    case "imageError":
      return {
        ...state,
        images: state.images.map((image) =>
          image.id === action.payload.id
            ? {
                ...image,
                isError: true,
                errorInfo: action.payload.error as Record<string, string>,
                loaded: true,
              }
            : image
        ),
      };

    default:
      return state;
  }
}

function processImages(
  state: ImageState,
  payload: { response: ImageGenerations; id: string }
): ImageState {
  const updatedImages = state.images.map((image) =>
    image.id === payload.id
      ? {
          ...image,
          generations: payload.response,
          loaded: true,
        }
      : image
  );

  return {
    ...state,
    images: updatedImages,
  };
}
