import { ClientPrincipalContextProvider } from "@aaronpowell/react-static-web-apps-auth";
import { FluentProvider, webLightTheme } from "@fluentui/react-components";
import React from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider, createBrowserRouter } from "react-router-dom";
import App from "./App";
import "./index.css";
import { Chat } from "./pages/Chat";
import { Image } from "./pages/Image";
import { Registration } from "./pages/event/Registration";
import { EventDataProvider } from "./providers/EventDataProvider";
import { OpenAIClientProvider } from "./providers/OpenAIProvider";
import { PromptErrorProvider } from "./providers/PromptErrorProvider";
import reportWebVitals from "./reportWebVitals";

const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
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
      <EventDataProvider>
        <OpenAIClientProvider>
          <PromptErrorProvider>
            <ClientPrincipalContextProvider>
              <RouterProvider router={router} />
            </ClientPrincipalContextProvider>
          </PromptErrorProvider>
        </OpenAIClientProvider>
      </EventDataProvider>
    </FluentProvider>
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
