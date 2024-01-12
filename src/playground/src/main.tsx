import { FluentProvider, webLightTheme } from "@fluentui/react-components";
import React from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider, createBrowserRouter } from "react-router-dom";
import "./index.css";
import {
  Registration,
  loader as registrationLoader,
  Layout as EventLayout,
  action as registrationAction,
} from "./pages/event";
import { Chat } from "./pages/playground/Chat";
import { Image } from "./pages/playground/Image";
import PlaygroundLayout from "./pages/playground/_Layout";
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
    ],
  },
  {
    path: "/event",
    element: <EventLayout />,
    children: [
      {
        path: ":id",
        element: <Registration />,
        loader: registrationLoader,
        action: registrationAction,
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
      <RouterProvider router={router} />
    </FluentProvider>
  </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
