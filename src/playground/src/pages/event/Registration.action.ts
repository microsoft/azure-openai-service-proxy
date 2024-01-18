import { ActionFunction } from "react-router-dom";
import { API_VERSION } from "../../constants";

export const action: ActionFunction = async ({ params }) => {
  const id = params.id;

  if (!id) {
    throw new Response("Invalid event id", { status: 404 });
  }

  const response = await fetch(
    `/api/${API_VERSION}/attendee/event/${id}/register`,
    {
      method: "POST",
    }
  );

  if (!response.ok) {
    throw new Response("Failed to register", { status: 500 });
  }

  return null;
};
