import { LoaderFunction } from "react-router-dom";
import { API_VERSION } from "../../constants";
import { toCamelCase } from "../../utils";

export const loader: LoaderFunction = async ({ params }) => {
  const id = params.id;

  if (!id) {
    throw new Response("Not Found", { status: 404 });
  }

  const [eventDetailsResponse, attendeeRegistrationResponse] =
    await Promise.all([
      fetch(`/api/${API_VERSION}/event/${id}`),
      fetch(`/api/${API_VERSION}/attendee/event/${id}`),
    ]);
  const eventDetails = await eventDetailsResponse.json();
  const attendeeRegistration = attendeeRegistrationResponse.ok
    ? await attendeeRegistrationResponse.json()
    : undefined;

  return {
    event: toCamelCase(eventDetails),
    attendee: toCamelCase(attendeeRegistration),
  };
};
