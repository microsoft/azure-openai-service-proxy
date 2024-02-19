/* eslint-disable react-refresh/only-export-components */
import React, {
  Dispatch,
  PropsWithChildren,
  createContext,
  useContext,
  useEffect,
  useState,
} from "react";
import { eventInfo } from "../api/eventInfo";

export type EventData = {
  name: string;
  url: string;
  urlText: string;
  maxTokenCap: number;
  capabilities: Record<string, string[]>;
  imageUrl?: string;
};

export enum AuthStatus {
  NotSet,
  Authorized,
  NotAuthorized,
}

export type EventDataContextValue = {
  eventData: EventData | undefined;
  setEventCode: Dispatch<string>;
  eventCodeSet: boolean;
  isAuthorized: boolean;
  eventCode?: string;
  authStatus: AuthStatus;
};

export const EventDataContext = createContext<EventDataContextValue>({
  eventData: undefined,
  setEventCode: () => {},
  isAuthorized: false,
  eventCodeSet: false,
  eventCode: undefined,
  authStatus: AuthStatus.NotSet,
});

const EVENT_CODE_STORAGE_KEY = "aoai:playground:eventCode";
const storage = localStorage;

const EventDataProvider: React.FC<PropsWithChildren> = ({ children }) => {
  const [eventData, setEventData] = useState<EventData | undefined>(undefined);
  const [authStatus, setAuthStatus] = useState<AuthStatus>(AuthStatus.NotSet);

  const [eventCode, setEventCode] = useState<string>(
    storage.getItem(EVENT_CODE_STORAGE_KEY) || ""
  );

  useEffect(() => {
    const abortController = new AbortController();
    const getEventData = async (eventCode: string) => {
      try {
        const data = await eventInfo(eventCode, abortController);
        setEventData(() => ({
          name: data.event_code,
          url: data.event_url,
          urlText: data.event_url_text,
          maxTokenCap: data.max_token_cap,
          capabilities: data.capabilities,
          imageUrl: data.event_image_url,
        }));
        if (data.is_authorized) {
          setAuthStatus(AuthStatus.Authorized);
          storage.setItem(EVENT_CODE_STORAGE_KEY, eventCode);
        } else {
          setAuthStatus(AuthStatus.NotAuthorized);
        }
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
      } catch (e: any) {
        if (e.name === "AbortError") {
          return;
        }
        setAuthStatus(AuthStatus.NotAuthorized);
      }
    };

    if (!eventCode) {
      storage.removeItem(EVENT_CODE_STORAGE_KEY);
      setAuthStatus(AuthStatus.NotSet);
      return;
    }

    getEventData(eventCode);

    return () => abortController.abort();
  }, [eventCode]);

  return (
    <EventDataContext.Provider
      value={{
        eventData,
        isAuthorized: authStatus === AuthStatus.Authorized,
        eventCodeSet: !!eventCode,
        authStatus,
        setEventCode: (eventCode) => setEventCode(() => eventCode),
        eventCode,
      }}
    >
      {eventData && children}
    </EventDataContext.Provider>
  );
};

const useEventDataContext = () => useContext(EventDataContext);

export { EventDataProvider, useEventDataContext };
