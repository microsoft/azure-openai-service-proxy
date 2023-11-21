import { Tab, TabList } from "@fluentui/react-components";
import { useLocation, useNavigate } from "react-router-dom";

export const Header = () => {
  const location = useLocation();
  const navigate = useNavigate();

  return (
    <TabList
      selectedValue={location.pathname === "/" ? "chat" : "images"}
      onTabSelect={(_, data) => {
        data.value === "chat" ? navigate("/") : navigate(`/${data.value}`);
      }}
    >
      <Tab value="chat">Chat</Tab>
      <Tab value="images">Image</Tab>
    </TabList>
  );
};
