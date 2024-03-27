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
import { Dispatch, useEffect, useReducer } from "react";
import ReactMarkdown from "react-markdown";
import { Form, useLoaderData } from "react-router-dom";
import { RegistrationAction, reducer } from "./Registration.reducers";
import type { AttendeeRegistration, EventDetails } from "./Registration.state";
import { adjustedLocalTime } from "../../adjustedLocalTime";

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

  return (
    <section className={styles.container}>
      <h1>{event?.eventCode}</h1>
      {event?.startTimestamp && event?.endTimestamp && event?.timeZoneLabel && (
        <EventTimeInfo event={event} />
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
        <AttendeeDetails
          attendee={attendee}
          styles={styles}
          dispatch={dispatch}
          showApiKey={state.showApiKey}
          notify={notify}
        />
      )}

      {state.profileLoaded && !state.profile && (
        <p>Please login to register.</p>
      )}

      <Toaster toasterId={toasterId} />
    </section>
  );
};

const AttendeeDetails = ({
  attendee,
  styles,
  dispatch,
  showApiKey,
  notify,
}: {
  attendee: AttendeeRegistration;
  styles: ReturnType<typeof useStyles>;
  dispatch: Dispatch<RegistrationAction>;
  showApiKey: boolean;
  notify: () => void;
}) => {
  const copyToClipboard = async (value: string) => {
    await navigator.clipboard.writeText(value);
    notify();
  };

  return (
    <>
      <div>
        <Field label="API Key" size="large">
          <div className={styles.apiKeyDisplay}>
            <Input
              name="apiKey"
              id="apiKey"
              type={showApiKey ? "text" : "password"}
              readOnly={true}
              value={attendee.apiKey}
              disabled={true}
            />
            <Button
              icon={showApiKey ? <EyeRegular /> : <EyeOffRegular />}
              onClick={() => dispatch({ type: "TOGGLE_API_KEY_VISIBILITY" })}
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

        <div style={{ marginTop: "10px" }}>
          <Form method="DELETE">
            <Button
              icon={<DeleteFilled />}
              className={styles.warningButton}
              type="submit"
            >
              Deregister from event.
            </Button>
          </Form>
        </div>
      </div>
    </>
  );
};

const EventTimeInfo = ({ event }: { event: EventDetails }) => {
  return (
    <div>
      <table>
        <tbody>
          <tr>
            <td>
              <strong>Starts:</strong>
            </td>
            <td>
              {adjustedLocalTime(event?.startTimestamp, event?.timeZoneOffset)}
            </td>
          </tr>
          <tr>
            <td>
              <strong>Ends:</strong>
            </td>
            <td>
              {adjustedLocalTime(event?.endTimestamp, event?.timeZoneOffset)}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  );
};
