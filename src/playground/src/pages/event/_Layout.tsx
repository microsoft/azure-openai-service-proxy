import { Outlet } from "react-router-dom";
import { makeStyles } from "@fluentui/react-components";
import { ClientPrincipalContextProvider } from "@aaronpowell/react-static-web-apps-auth";
import { Header } from "../../components/event/Header";

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

export function Layout() {
  const styles = useStyles();

  return (
    <ClientPrincipalContextProvider>
      <div className={styles.container}>
        <nav>
          <Header />
        </nav>
        <Outlet />
      </div>
    </ClientPrincipalContextProvider>
  );
}
