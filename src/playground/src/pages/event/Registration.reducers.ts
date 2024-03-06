import { RegistrationState } from "./Registration.state";

export type RegistrationAction =
  | {
      type: "PROFILE_LOADED";
      payload: {
        loaded: RegistrationState["profileLoaded"];
        profile: RegistrationState["profile"];
      };
    }
  | { type: "TOGGLE_API_KEY_VISIBILITY" };

export const reducer = (
  state: RegistrationState,
  action: RegistrationAction
) => {
  switch (action.type) {
    case "PROFILE_LOADED":
      return {
        ...state,
        profileLoaded: action.payload.loaded,
        profile: action.payload.profile,
      };

    case "TOGGLE_API_KEY_VISIBILITY":
      return {
        ...state,
        showApiKey: !state.showApiKey,
      };

    default:
      return state;
  }
};
