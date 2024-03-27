import { ActionFunction } from "react-router-dom";
import { API_VERSION } from "../../constants";

export const action: ActionFunction = async ({ params, request }) => {
  const id = params.id;

  if (!id) {
    throw new Response("Invalid event id", { status: 404 });
  }

  if (request.method === "POST") {
    await registerUser(id);
  } else if (request.method === "DELETE") {
    await deregisterUser(id);
  }

  return null;
};

async function registerUser(id: string) {
  const response = await fetch(
    `/api/${API_VERSION}/attendee/event/${id}/register`,
    {
      method: "POST",
    }
  );

  if (!response.ok) {
    throw new Response("Failed to register", { status: 500 });
  }
}

async function deregisterUser(id: string) {
  const response = await fetch(
    `/api/${API_VERSION}/attendee/event/${id}/register`,
    {
      method: "DELETE",
    }
  );

  if (!response.ok) {
    throw new Response("Failed to deregister", { status: 500 });
  }
}
