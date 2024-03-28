import { ActionFunction, redirectDocument } from "react-router-dom";
import { API_VERSION } from "../../constants";

export const action: ActionFunction = async ({ request }) => {
  if (request.method === "DELETE") {
    const response = await fetch(`/api/${API_VERSION}/attendee`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Response("Failed to purge", { status: 500 });
    }

    const formData = await request.formData();
    const redirectUrl = formData.get("redirectUrl") as string;

    return redirectDocument(redirectUrl);
  }

  return null;
};
