import { ClientPrincipalContextProvider } from "@aaronpowell/react-static-web-apps-auth";
import { FluentProvider, webLightTheme } from "@fluentui/react-components";
import React from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider, createBrowserRouter } from "react-router-dom";
import PlaygroundLayout from "./pages/playground/_Layout";
import "./index.css";
import { Chat } from "./pages/playground/Chat";
import { Image } from "./pages/playground/Image";
import { Registration } from "./pages/event/Registration";
import reportWebVitals from "./reportWebVitals";

const router = createBrowserRouter([
  {
    path: "/",
    element: <PlaygroundLayout />,
    children: [
      {
        path: "/",
        element: <Chat />,
      },
      {
        path: "/images",
        element: <Image />,
      },
      {
        path: "/event/:id",
        element: <Registration />,
      },
    ],
  },
]);

const root = ReactDOM.createRoot(
  document.getElementById("root") as HTMLElement
);
root.render(
  <React.StrictMode>
    <FluentProvider theme={webLightTheme}>
      <ClientPrincipalContextProvider>
        <RouterProvider router={router} />
      </ClientPrincipalContextProvider>
    </FluentProvider>
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
