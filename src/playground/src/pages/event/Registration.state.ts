import { ClientPrincipal } from "@aaronpowell/react-static-web-apps-auth";

export type EventDetails = {
  id: string;
  eventCode: string;
  eventMarkdown: string;
  startTimestamp: Date;
  endTimestamp: Date;
  timeZoneLabel: string;
  timeZoneOffset: number;
  organizerName: string;
  organizerEmail: string;
};

export type AttendeeRegistration = { apiKey: string; active: boolean };

export type RegistrationState = {
  profileLoaded: boolean;
  profile?: ClientPrincipal;
  showApiKey: boolean;
};
