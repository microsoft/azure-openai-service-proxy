import { Unauthorised } from "./components/Unauthorised";
import { Error } from "./components/Error";
import { Outlet } from "react-router-dom";
import { Header } from "./components/Headers";
import { makeStyles } from "@fluentui/react-components";

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

function App() {
  const styles = useStyles();

  return (
    <div className={styles.container}>
      <nav>
        <Header />
      </nav>
      <Outlet />
      <Unauthorised />
      <Error />
    </div>
  );
}

export default App;
