import React from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import App from "./App";
import reportWebVitals from "./reportWebVitals";
import { FluentProvider, webLightTheme } from "@fluentui/react-components";
import { EventDataProvider } from "./providers/EventDataProvider";
import { PromptErrorProvider } from "./providers/PromptErrorProvider";
import { RouterProvider, createBrowserRouter } from "react-router-dom";
import { Chat } from "./pages/Chat";
import { OpenAIClientProvider } from "./providers/OpenAIProvider";

const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      {
        path: "/",
        element: <Chat />,
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
            <RouterProvider router={router} />
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
