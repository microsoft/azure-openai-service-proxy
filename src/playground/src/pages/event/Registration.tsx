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
import {
  CopyRegular,
  DeleteFilled,
  EyeOffRegular,
  EyeRegular,
} from "@fluentui/react-icons";
import { tokens } from "@fluentui/react-theme";
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
  warningButton: {
    color: tokens.colorStatusDangerForeground1,
    backgroundColor: tokens.colorStatusDangerBackground1,
  },
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
          Copied to clipboard.
        </ToastTitle>
      </Toast>,
      { position: "top", intent: "success" }
    );

  const copyToClipboard = async (value: string) => {
    await navigator.clipboard.writeText(value);
    notify();
  };

  const adjustedLocalTime = (
    timestamp: Date,
    utcOffsetInMinutes: number
  ): string => {
    // returns time zone adjusted date/time
    const date = new Date(timestamp);
    // get the timezone offset component that was added as no tz supplied in date time
    const tz = date.getTimezoneOffset();
    // remove the browser based timezone offset
    date.setMinutes(date.getMinutes() - tz);
    // add the event timezone offset
    date.setMinutes(date.getMinutes() - utcOffsetInMinutes);

    // Get the browser locale
    const locale = navigator.language || navigator.languages[0];

    // Specify the formatting options
    const options: Intl.DateTimeFormatOptions = {
      weekday: "long",
      year: "numeric",
      month: "long",
      day: "numeric",
      hour: "numeric",
      minute: "numeric",
    };

    // Create an Intl.DateTimeFormat object
    const formatter = new Intl.DateTimeFormat(locale, options);
    // Format the date
    const formattedDate = formatter.format(date);
    return formattedDate;
  };

  return (
    <section className={styles.container}>
      <h1>{event?.eventCode}</h1>
      {event?.startTimestamp && event?.endTimestamp && event?.timeZoneLabel && (
        <div>
          <table>
            <tbody>
              <tr>
                <td>
                  <strong>Starts:</strong>
                </td>
                <td>
                  {adjustedLocalTime(
                    event?.startTimestamp,
                    event?.timeZoneOffset
                  )}
                </td>
              </tr>
              <tr>
                <td>
                  <strong>Ends:</strong>
                </td>
                <td>
                  {adjustedLocalTime(
                    event?.endTimestamp,
                    event?.timeZoneOffset
                  )}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      )}
      <div style={{ textAlign: "center", padding: "40px" }}>
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
        <>
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
                  onClick={() =>
                    dispatch({ type: "TOGGLE_API_KEY_VISIBILITY" })
                  }
                />
                <Button
                  icon={<CopyRegular />}
                  onClick={() => copyToClipboard(attendee.apiKey)}
                />
              </div>
            </Field>
            <div>
              <p>
                Copy the API Key, then navigate to the{" "}
                <Link href={`${window.location.origin}`}>Playground</Link>.
              </p>
            </div>
            <Field label="Endpoint" size="large">
              <div className={styles.apiKeyDisplay}>
                <Input
                  name="endpoint"
                  id="endpoint"
                  type="text"
                  readOnly={true}
                  value={`${window.location.origin}/api/v1`}
                  disabled={true}
                />
                <Button
                  icon={<CopyRegular />}
                  onClick={() =>
                    copyToClipboard(`${window.location.origin}/api/v1`)
                  }
                />
              </div>
            </Field>

            <Field>
              <p>
                <Button
                  icon={<DeleteFilled />}
                  className={styles.warningButton}
                >
                  Deregister from event.
                </Button>
              </p>
            </Field>
          </div>
        </>
      )}

      {state.profileLoaded && !state.profile && (
        <p>Please login to register.</p>
      )}

      <Toaster toasterId={toasterId} />
    </section>
  );
};
