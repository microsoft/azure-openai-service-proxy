import { GetImagesOptions, ImageGenerations } from "@azure/openai";

export type ExtendedImageGenerations = {
  prompt: string;
  loaded: boolean;
  id: string;
  generations?: ImageGenerations;
  isError: boolean;
  errorInfo?: Record<string, string>;
};

export type ImageState = {
  model?: string;
  parameters: GetImagesOptions;
  images: ExtendedImageGenerations[];
};

export const INITIAL_STATE: ImageState = {
  parameters: {
    n: 1,
    responseFormat: "url",
    size: "1024x1024",
  },
  images: [],
};
