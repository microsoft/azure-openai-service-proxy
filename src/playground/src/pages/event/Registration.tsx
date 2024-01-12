import type { EventDetails } from "./Registration.state";
import { useEffect, useReducer } from "react";
import { INITIAL_STATE } from "./Registration.state";
import { reducer } from "./Registration.reducers";
import {
  StaticWebAuthLogins,
  useClientPrincipal,
} from "@aaronpowell/react-static-web-apps-auth";
import { Form, useLoaderData, useSubmit } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import { Button } from "@fluentui/react-components";

export const Registration = () => {
  const eventDetails = useLoaderData() as EventDetails;

  const [state, dispatch] = useReducer(reducer, {
    ...INITIAL_STATE,
    eventDetails,
  });
  const { loaded, clientPrincipal } = useClientPrincipal();

  useEffect(() => {
    dispatch({
      type: "PROFILE_LOADED",
      payload: { loaded, profile: clientPrincipal || undefined },
    });
  }, [loaded, clientPrincipal]);

  return (
    <section>
      <h1>{state.eventDetails?.eventCode}</h1>
      <div>
        <ReactMarkdown>{state.eventDetails?.eventMarkdown}</ReactMarkdown>
      </div>
      {state.profileLoaded && state.profile && (
        <div>
          <Form method="post">
            <Button type="submit">Register</Button>
          </Form>
        </div>
      )}
      {state.profileLoaded && !state.profile && (
        <StaticWebAuthLogins
          azureAD={false}
          twitter={false}
          postLoginRedirect={window.location.href}
        />
      )}
    </section>
  );
};
