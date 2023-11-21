import {
  PropsWithChildren,
  createContext,
  useContext,
  useEffect,
  useState,
} from "react";
import { OpenAIClient, AzureKeyCredential } from "@azure/openai";
import { useEventDataContext } from "./EventDataProvider";
import { API_VERSION } from "../constants";

export type OpenAIProviderValue = {
  client?: OpenAIClient;
};

const OpenAIClientContext = createContext<OpenAIProviderValue>({
  client: undefined,
});

const OpenAIClientProvider: React.FC<PropsWithChildren> = ({ children }) => {
  const { eventCode } = useEventDataContext();
  const [client, setClient] = useState<OpenAIClient | undefined>(undefined);

  useEffect(() => {
    if (eventCode) {
      setClient(
        new OpenAIClient(
          `${window.location.origin}/${API_VERSION}/api`,
          new AzureKeyCredential(`${eventCode}`),
          {
            allowInsecureConnection: true,
          }
        )
      );
    } else {
      setClient(undefined);
    }
  }, [eventCode]);

  return (
    <OpenAIClientContext.Provider value={{ client }}>
      {children}
    </OpenAIClientContext.Provider>
  );
};

const useOpenAIClientContext = () => useContext(OpenAIClientContext);

export { OpenAIClientProvider, useOpenAIClientContext };
