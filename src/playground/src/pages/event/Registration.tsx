import { useClientPrincipal } from "@aaronpowell/react-static-web-apps-auth";
import {
  Button,
  Field,
  Input,
  Link,
  Toast,
  ToastTitle,
  ToastTrigger,
  Toaster,
  makeStyles,
  shorthands,
  useId,
  useToastController,
} from "@fluentui/react-components";
import { CopyRegular, EyeOffRegular, EyeRegular } from "@fluentui/react-icons";
import { useEffect, useReducer } from "react";
import ReactMarkdown from "react-markdown";
import { Form, useLoaderData } from "react-router-dom";
import { reducer } from "./Registration.reducers";
import type { AttendeeRegistration, EventDetails } from "./Registration.state";

const useStyles = makeStyles({
  container: {
    display: "flex",
    flexDirection: "column",
    justifyContent: "start",
    alignItems: "center",
    height: "100vh",
    ...shorthands.padding("0", "var(--global-margin)"),
  },
  apiKeyDisplay: { display: "flex", alignItems: "center", columnGap: "4px" },
});

export const Registration = () => {
  const { event, attendee } = useLoaderData() as {
    event: EventDetails;
    attendee?: AttendeeRegistration;
  };

  const styles = useStyles();

  const [state, dispatch] = useReducer(reducer, {
    profileLoaded: false,
    showApiKey: false,
  });
  const { loaded, clientPrincipal } = useClientPrincipal();

  useEffect(() => {
    dispatch({
      type: "PROFILE_LOADED",
      payload: { loaded, profile: clientPrincipal || undefined },
    });
  }, [loaded, clientPrincipal]);

  const toasterId = useId("toaster");
  const { dispatchToast } = useToastController(toasterId);

  const notify = () =>
    dispatchToast(
      <Toast>
        <ToastTitle
          action={
            <ToastTrigger>
              <Link>Dismiss</Link>
            </ToastTrigger>
          }
        >
          API Key copied.
        </ToastTitle>
      </Toast>,
      { position: "top", intent: "success" }
    );

  const copyToClipboard = async () => {
    await navigator.clipboard.writeText(attendee!.apiKey);
    notify();
  };

  const adjustedLocalTime = (timestamp: Date, utcOffsetInMinutes: number): Date => {
    // returns time zone adjusted date/time
    const date = new Date(timestamp);
    // get the timezone offset component that was added as no tz supplied in date time
    const tz = date.getTimezoneOffset();
    // remove the browser based timezone offset
    date.setMinutes(date.getMinutes() - tz);
    // add the event timezone offset
    date.setMinutes(date.getMinutes() - utcOffsetInMinutes);

    return date;
  };

  return (
    <section className={styles.container}>
      <h1>{event?.eventCode}</h1>
      {event?.startTimestamp && event?.endTimestamp && event?.timeZoneLabel && (
        <div>
          <p>
            <strong>Starts:</strong> {adjustedLocalTime(event?.startTimestamp, event?.timeZoneOffset).toLocaleString()},&nbsp;
            <strong>Ends:</strong> {adjustedLocalTime(event?.endTimestamp, event?.timeZoneOffset).toLocaleString()},&nbsp;
          </p>
        </div>
      )}
      <div>
        <ReactMarkdown>{event?.eventMarkdown}</ReactMarkdown>
      </div>
      {state.profileLoaded && state.profile && !attendee && (
        <div>
          <Form method="post">
            <Button type="submit">Register</Button>
          </Form>
        </div>
      )}
      {state.profileLoaded && state.profile && attendee && (
        <div>
          <Field label="API Key" size="large">
            <div className={styles.apiKeyDisplay}>
              <Input
                name="apiKey"
                id="apiKey"
                type={state.showApiKey ? "text" : "password"}
                readOnly={true}
                value={attendee.apiKey}
                disabled={true}
              />
              <Button
                icon={state.showApiKey ? <EyeRegular /> : <EyeOffRegular />}
                onClick={() => dispatch({ type: "TOGGLE_API_KEY_VISIBILITY" })}
              />
              <Button icon={<CopyRegular />} onClick={copyToClipboard} />
            </div>
          </Field>
        </div>
      )}

      {state.profileLoaded && !state.profile && (
        <p>Please login to register.</p>
      )}

      <Toaster toasterId={toasterId} />
    </section>
  );
};
