import React, {
  PropsWithChildren,
  createContext,
  useContext,
  useEffect,
  useState,
} from "react";
import { eventInfo } from "./api/eventInfo";

type EventData = {
  name: string;
  url: string;
  url_text: string;
  max_token_cap: number;
};

export enum AuthStatus {
  NotSet,
  Authorized,
  NotAuthorized,
}

export type EventDataContextValue = {
  eventData: EventData | undefined;
  setEventCode: React.Dispatch<string>;
  eventCodeSet: boolean;
  isAuthorized: boolean;
  eventCode?: string;
  authStatus: AuthStatus;
};

export const EventDataContext = createContext<EventDataContextValue>({
  eventData: undefined,
  setEventCode: (code: string) => {},
  isAuthorized: false,
  eventCodeSet: false,
  eventCode: undefined,
  authStatus: AuthStatus.NotSet,
});

const EventDataProvider: React.FC<PropsWithChildren> = ({ children }) => {
  const [eventData, setEventData] = useState<EventData | undefined>(undefined);
  const [eventCode, setEventCode] = useState<string>("");
  const [authStatus, setAuthStatus] = useState<AuthStatus>(AuthStatus.NotSet);

  useEffect(() => {
    const getEventData = async (eventCode: string) => {
      try {
        const data = await eventInfo(eventCode);
        setEventData(() => ({
          name: data.event_name,
          url: data.event_url,
          url_text: data.event_url_text,
          max_token_cap: data.max_token_cap,
        }));
        if (data.is_authorized) {
          setAuthStatus(AuthStatus.Authorized);
        } else {
          setAuthStatus(AuthStatus.NotAuthorized);
        }
      } catch (e) {
        setAuthStatus(AuthStatus.NotAuthorized);
      }
    };

    if (!eventCode) {
      setAuthStatus(AuthStatus.NotSet);
      return;
    }

    getEventData(eventCode);
  }, [eventCode]);

  return (
    <EventDataContext.Provider
      value={{
        eventData,
        setEventCode,
        isAuthorized: authStatus === AuthStatus.Authorized,
        eventCodeSet: !!eventCode,
        eventCode,
        authStatus,
      }}
    >
      {children}
    </EventDataContext.Provider>
  );
};

const useEventDataContext = () => useContext(EventDataContext);

export { EventDataProvider, useEventDataContext };
