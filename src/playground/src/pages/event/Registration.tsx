import type { AttendeeRegistration, EventDetails } from "./Registration.state";
import { useEffect, useReducer } from "react";
import { reducer } from "./Registration.reducers";
import {
  Logout,
  StaticWebAuthLogins,
  useClientPrincipal,
} from "@aaronpowell/react-static-web-apps-auth";
import { Form, useLoaderData } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import { Button, Input, Label } from "@fluentui/react-components";
import { CopyRegular, EyeOffRegular, EyeRegular } from "@fluentui/react-icons";

export const Registration = () => {
  const { event, attendee } = useLoaderData() as {
    event: EventDetails;
    attendee?: AttendeeRegistration;
  };

  console.log({ event, attendee });

  const [state, dispatch] = useReducer(reducer, {
    profileLoaded: false,
    showApiKey: false,
    event,
    attendee,
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
      <h1>{state.event?.eventCode}</h1>
      <div>
        <ReactMarkdown>{state.event?.eventMarkdown}</ReactMarkdown>
      </div>
      {state.profileLoaded && state.profile && !state.attendee && (
        <div>
          <Form method="post">
            <Button type="submit">Register</Button>
          </Form>
        </div>
      )}
      {state.profileLoaded && state.profile && state.attendee && (
        <div>
          <Label htmlFor="apiKey">API Key</Label>
          <Input
            name="apiKey"
            id="apiKey"
            type={state.showApiKey ? "text" : "password"}
            readOnly={true}
            value={state.attendee.apiKey}
            disabled={true}
          />
          <Button
            icon={state.showApiKey ? <EyeRegular /> : <EyeOffRegular />}
            onClick={() => dispatch({ type: "TOGGLE_API_KEY_VISIBILITY" })}
          />
          <Button
            icon={<CopyRegular />}
            onClick={() =>
              navigator.clipboard.writeText(state.attendee!.apiKey)
            }
          />
          <br />
          <Logout postLogoutRedirect={window.location.href} />
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
