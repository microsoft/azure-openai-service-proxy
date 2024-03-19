import { GetImagesOptions, ImageGenerations } from "@azure/openai";

export type ImageState = {
  prompt?: string;
  isLoading: boolean;
  model?: string;
  parameters: GetImagesOptions;
  images: ImageGenerations[];
};

export const INITIAL_STATE: ImageState = {
  isLoading: false,
  parameters: {
    n: 1,
    responseFormat: "url",
    size: "1024x1024",
  },
  images: [],
};
