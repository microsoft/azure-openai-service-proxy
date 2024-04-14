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
    ...shorthands.margin("0px", "140px"),
    fontSize: "medium",
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
    <section className={styles.container} >
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
      <div style={{ textAlign: "left", padding: "0px" }}>
        <ReactMarkdown>{event?.eventMarkdown}</ReactMarkdown>
      </div>
      {state.profileLoaded && state.profile && !attendee && (
        <div>
          <Form method="post">
            <Button type="submit" style={{ fontSize: "medium" }} appearance="primary">
              Register
            </Button>
          </Form>
        </div>
      )}
      {state.profileLoaded && state.profile && attendee && (
        <>
          <h2>Registration Details</h2>

          <h3>AI Proxy Playground Access</h3>
          <p>
          You'll need the API Key to access the AI Proxy Playground.
          </p>
          <div>
            <Field label="API Key" size="large">
              <div className={styles.apiKeyDisplay}>
                <Input
                  name="apiKey"
                  id="apiKey"
                  value={attendee.apiKey}
                  disabled={true}
                  type={state.showApiKey ? "text" : "password"}
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
              <ol>
                <li>Copy the API Key. </li>
                <li>When you navigate to the AI Proxy Playground, paste the API Key and Authorize.
                </li>
                <li>Navigate to the{" "}
                  <Link href={`${window.location.origin}`} target="_blank" rel="noopener noreferrer">AI Proxy Playground</Link>.</li>
              </ol>
            </div>
            <h3>SDK Access</h3>
            <p>
            You'll need both the API Key and Endpoint to access the AI Proxy Playground using an SDK.
            </p>
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
              <h3>Python example using the OpenAI Python SDK</h3>
              <pre >
                <code>
                  {`# pip install openai

from openai import AzureOpenAI

ENDPOINT = "${window.location.origin}/api/v1"
API_KEY = "<YOUR_API_KEY>"

API_VERSION = "2024-02-01"
MODEL_NAME = "gpt-35-turbo"

client = AzureOpenAI(
    azure_endpoint=ENDPOINT,
    api_key=API_KEY,
    api_version=API_VERSION,
)

MESSAGES = [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Who won the world series in 2020?"},
    {
        "role": "assistant",
        "content": "The Los Angeles Dodgers won the World Series in 2020.",
    },
    {"role": "user", "content": "Where was it played?"},
]

completion = client.chat.completions.create(
    model=MODEL_NAME,
    messages=MESSAGES,
)

print(completion.model_dump_json(indent=2))
                  `}
                </code>
              </pre>
              <h3>More examples</h3>
              <ul>
                <li>
                  <Link
                    href="https://learn.microsoft.com/azure/ai-services/openai/quickstart"
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    Quickstart: Get started generating text using Azure OpenAI Service
                  </Link>
                </li>
                <li>
                  <Link
                    href="https://github.com/microsoft/azure-openai-service-proxy/tree/main/examples"
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    Azure OpenAI Service Proxy Examples
                  </Link>
                </li>
              </ul>
              <br />
            </Field>
          </div>
        </>
      )}

      {state.profileLoaded && !state.profile && (
        <h2>Please login to register.</h2>
      )}

      <Toaster toasterId={toasterId} />
    </section>
  );
};
