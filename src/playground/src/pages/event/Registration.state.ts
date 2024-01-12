import { ClientPrincipal } from "@aaronpowell/react-static-web-apps-auth";

export type EventDetails = {
  id: string;
  eventCode: string;
  eventMarkdown: string;
  startDate: Date;
  endDate: Date;
  organizerName: string;
  organizerEmail: string;
};

export type RegistrationState = {
  profileLoaded: boolean;
  profile?: ClientPrincipal;
  eventDetails?: EventDetails;
};

export const INITIAL_STATE: RegistrationState = {
  profileLoaded: false,
};
