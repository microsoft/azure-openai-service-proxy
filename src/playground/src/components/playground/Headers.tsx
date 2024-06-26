import {
  Tab,
  TabList,
  makeStyles,
  shorthands,
} from "@fluentui/react-components";
import { useLocation, useNavigate } from "react-router-dom";
import { ApiKeyInput } from "./controls/EventCodeInput";
import {
  EventData,
  useEventDataContext,
} from "../../providers/EventDataProvider";

const useStyles = makeStyles({
  container: {
    display: "grid",
    gridTemplateColumns: "2fr 1fr",
  },
  right: {
    justifySelf: "right",
    ...shorthands.padding("0px", "20px", "0px", "0px"),
  },
  header: {
    height: "48px",
    float: "left",
    marginRight: "24px",
    ...shorthands.padding("14px", "0px", "0px", "15px"),
  },
});

const hasCapability = (
  eventData: EventData | undefined,
  capability: string
) => {
  if (!eventData) {
    return false;
  }
  const capabilities = eventData.capabilities[capability];
  return capabilities && capabilities.length > 0;
};

export const Header = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const styles = useStyles();
  const { eventData, isAuthorized } = useEventDataContext();

  return (
    <div className={styles.container}>
      <div>
        <div className={styles.header}>
          {isAuthorized && (
            <>
              <img
                src={eventData?.imageUrl ? eventData?.imageUrl : "/logo.png"}
                style={{ height: "24px" }}
              />
              <br />
              {/* {eventData!.url.length !== 0 && (
                <a
                  href={eventData!.url}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  {eventData!.name}
                </a>)} */}

              {eventData!.name}
            </>
          )}
          {!isAuthorized && (
            <>
              <img src={"/logo.png"} style={{ height: "24px" }} />
            </>
          )}
        </div>

        <TabList
          selectedValue={location.pathname === "/" ? "chat" : "images"}
          onTabSelect={(_, data) => {
            data.value === "chat" ? navigate("/") : navigate(`/${data.value}`);
          }}
        >
          {isAuthorized && (
            <>
              {hasCapability(eventData, "openai-chat") && (
                <Tab value="chat">Chat</Tab>
              )}
              {hasCapability(eventData, "openai-dalle3") && (
                <Tab value="images">Image</Tab>
              )}
            </>
          )}
        </TabList>
      </div>
      <div className={styles.right}>
        <ApiKeyInput />
      </div>
    </div>
  );
};
