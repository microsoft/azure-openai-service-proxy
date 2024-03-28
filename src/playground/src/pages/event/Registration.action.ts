import { ActionFunction } from "react-router-dom";
import { API_VERSION } from "../../constants";

export const action: ActionFunction = async ({ params, request }) => {
  const id = params.id;

  if (!id) {
    throw new Response("Invalid event id", { status: 404 });
  }

  switch (request.method) {
    case "POST":
      await registerUser(id);
      break;
    case "DELETE":
      await deregisterUser(id);
      break;
    case "PATCH":
      await activateUser(id);
      break;
    default:
      throw new Response("Invalid request method", { status: 405 });
  }

  return null;
};

const registerUser = (id: string) => executeRequest(id, "POST");
const deregisterUser = (id: string) => executeRequest(id, "DELETE");
const activateUser = (id: string) => executeRequest(id, "PATCH");

async function executeRequest(id: string, method: "POST" | "DELETE" | "PATCH") {
  const response = await fetch(
    `/api/${API_VERSION}/attendee/event/${id}/register`,
    {
      method,
    }
  );

  if (!response.ok) {
    throw new Response(`Failed to ${method}`, { status: 500 });
  }
}
