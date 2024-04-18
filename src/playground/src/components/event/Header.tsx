import {
  Logout,
  StaticWebAuthLogins,
  useClientPrincipal,
} from "@aaronpowell/react-static-web-apps-auth";
import {
  Button,
  Link,
  makeStyles,
  shorthands,
} from "@fluentui/react-components";
import {} from "@fluentui/react-icons";

const useStyles = makeStyles({
  container: {
    display: "grid",
    gridTemplateColumns: "1fr 2fr 1fr",
    textAlign: "center",
  },
  right: {
    justifySelf: "right",
    ...shorthands.padding("0px", "20px", "0px", "0px"),
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    fontSize: "medium",
  },
  left: {
    justifySelf: "left",
    ...shorthands.padding("0px", "0px", "0px", "20px"),
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
  },
  logo: {
    height: "24px",
  },
});

export const Header = () => {
  const styles = useStyles();

  const { loaded, clientPrincipal } = useClientPrincipal();

  return (
    <div className={styles.container}>
      <div className={styles.left}>
        <Link
          href="https://github.com/microsoft/azure-openai-service-proxy"
          title="GitHub repo"
          target="_blank"
        >
          <img
            src="/github-mark.svg"
            alt="GitHub repo"
            className={styles.logo}
          />
        </Link>
      </div>
      <h1>Event Registration</h1>
      <div className={styles.right}>
        {loaded && clientPrincipal && (
          <>
            Welcome {clientPrincipal.userDetails}&nbsp;
            <Logout
              postLogoutRedirect={window.location.href}
              customRenderer={({ href }) => (
                <Button href={href} as="a" appearance="primary">
                  Logout
                </Button>
              )}
            />
          </>
        )}

        {loaded && !clientPrincipal && (
          <StaticWebAuthLogins
            azureAD={false}
            twitter={false}
            postLoginRedirect={window.location.href}
            customRenderer={(props) => {
              return (
                <Button href={props.href} as="a" appearance="primary">
                  Login with {props.name}
                </Button>
              );
            }}
          />
        )}
      </div>
    </div>
  );
};
