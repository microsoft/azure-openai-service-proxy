import { LoaderFunction } from "react-router-dom";
import { API_VERSION } from "../../constants";
import { toCamelCase } from "../../utils";

export const loader: LoaderFunction = async ({ params }) => {
  const id = params.id;

  if (!id) {
    throw new Response("Not Found", { status: 404 });
  }

  const response = await fetch(`/api/${API_VERSION}/event/${id}`);
  const eventDetails = await response.json();

  return toCamelCase(eventDetails);
};
