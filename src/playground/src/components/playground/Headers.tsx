import {
  Tab,
  TabList,
  makeStyles,
  shorthands,
} from "@fluentui/react-components";
import { useLocation, useNavigate } from "react-router-dom";
import { ApiKeyInput } from "./EventCodeInput";

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

export const Header = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const styles = useStyles();

  return (
    <>
      <div className={styles.container}>
        <TabList
          selectedValue={location.pathname === "/" ? "chat" : "images"}
          onTabSelect={(_, data) => {
            data.value === "chat" ? navigate("/") : navigate(`/${data.value}`);
          }}
        >
          <Tab value="chat">Chat</Tab>
          {/* <Tab value="images">Image</Tab> */}
        </TabList>
        <div className={styles.right}>
          <ApiKeyInput />
        </div>
      </div>
    </>
  );
};
