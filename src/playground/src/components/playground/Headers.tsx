import {
  Tab,
  TabList,
  makeStyles,
  shorthands,
} from "@fluentui/react-components";
import { useLocation, useNavigate } from "react-router-dom";
import { ApiKeyInput } from "./EventCodeInput";
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
  const { eventData } = useEventDataContext();

  return (
    <>
      <div className={styles.container}>
        <TabList
          selectedValue={location.pathname === "/" ? "chat" : "images"}
          onTabSelect={(_, data) => {
            data.value === "chat" ? navigate("/") : navigate(`/${data.value}`);
          }}
        >
          {hasCapability(eventData, "openai-chat") && (
            <Tab value="chat">Chat</Tab>
          )}
          {hasCapability(eventData, "openai-image") && (
            <Tab value="images">Image</Tab>
          )}
        </TabList>
        <div className={styles.right}>
          <ApiKeyInput />
        </div>
      </div>
    </>
  );
};
