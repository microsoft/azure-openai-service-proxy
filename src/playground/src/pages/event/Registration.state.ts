import { ClientPrincipal } from "@aaronpowell/react-static-web-apps-auth";

export type RegistrationState = {
  profileLoaded: boolean;
  profile: null | ClientPrincipal;
};

export const INITIAL_STATE: RegistrationState = {
  profileLoaded: false,
  profile: null,
};
