import { useEffect, useReducer } from "react";
import { INITIAL_STATE } from "./Registration.state";
import { reducer } from "./Registration.reducers";
import { useClientPrincipal } from "@aaronpowell/react-static-web-apps-auth";

export const Registration = () => {
  const [state, dispatch] = useReducer(reducer, INITIAL_STATE);
  const { loaded, clientPrincipal } = useClientPrincipal();

  console.log(state);

  useEffect(() => {
    dispatch({
      type: "PROFILE_LOADED",
      payload: { loaded, profile: clientPrincipal },
    });
  }, [loaded, clientPrincipal]);

  return (
    <section>
      <h1>Register</h1>
    </section>
  );
};
