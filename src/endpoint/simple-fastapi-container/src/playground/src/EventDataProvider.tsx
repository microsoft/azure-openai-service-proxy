import React, {
  Dispatch,
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
  setEventConnection: Dispatch<{ eventCode: string; endpoint: string }>;
  eventCodeSet: boolean;
  endpointSet: boolean;
  isAuthorized: boolean;
  eventCode?: string;
  authStatus: AuthStatus;
  endpoint?: string;
};

export const EventDataContext = createContext<EventDataContextValue>({
  eventData: undefined,
  setEventConnection: () => {},
  isAuthorized: false,
  eventCodeSet: false,
  endpointSet: false,
  eventCode: undefined,
  authStatus: AuthStatus.NotSet,
});

const EventDataProvider: React.FC<PropsWithChildren> = ({ children }) => {
  const [eventData, setEventData] = useState<EventData | undefined>(undefined);
  const [authStatus, setAuthStatus] = useState<AuthStatus>(AuthStatus.NotSet);

  const [eventConnection, setEventConnection] = useState<{
    eventCode: string;
    endpoint: string;
  }>({ eventCode: "", endpoint: "" });

  useEffect(() => {
    const getEventData = async (eventCode: string, endpoint: string) => {
      try {
        const data = await eventInfo(eventCode, endpoint);
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

    if (!eventConnection.eventCode || !eventConnection.endpoint) {
      setAuthStatus(AuthStatus.NotSet);
      return;
    }

    getEventData(eventConnection.eventCode, eventConnection.endpoint);
  }, [eventConnection]);

  return (
    <EventDataContext.Provider
      value={{
        eventData,
        isAuthorized: authStatus === AuthStatus.Authorized,
        eventCodeSet: !!eventConnection.eventCode,
        eventCode: eventConnection.eventCode,
        authStatus,
        endpoint: eventConnection.endpoint,
        endpointSet: !!eventConnection.endpoint,
        setEventConnection: ({ eventCode, endpoint }) =>
          setEventConnection(() => ({ eventCode, endpoint })),
      }}
    >
      {children}
    </EventDataContext.Provider>
  );
};

const useEventDataContext = () => useContext(EventDataContext);

export { EventDataProvider, useEventDataContext };
