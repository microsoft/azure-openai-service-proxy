import { Unauthorised } from "../../components/playground/Unauthorised";
import { Error } from "../../components/playground/Error";
import { Outlet } from "react-router-dom";
import { Header } from "../../components/playground/Headers";
import { makeStyles } from "@fluentui/react-components";
import { EventDataProvider } from "../../providers/EventDataProvider";
import { OpenAIClientProvider } from "../../providers/OpenAIProvider";
import { PromptErrorProvider } from "../../providers/PromptErrorProvider";

const useStyles = makeStyles({
  container: {
    display: "grid",
    gridGap: "1px",
    height: "100vh",
    gridTemplateAreas: `
      header
      main
    `,
    gridTemplateRows: "min-content 1fr",
  },
});

function Layout() {
  const styles = useStyles();

  return (
    <EventDataProvider>
      <OpenAIClientProvider>
        <PromptErrorProvider>
          <div className={styles.container}>
            <nav>
              <Header />
            </nav>
            <Outlet />
            <Unauthorised />
            <Error />
          </div>
        </PromptErrorProvider>
      </OpenAIClientProvider>
    </EventDataProvider>
  );
}

export default Layout;
