import {
  Logout,
  StaticWebAuthLogins,
  useClientPrincipal,
} from "@aaronpowell/react-static-web-apps-auth";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogSurface,
  DialogTitle,
  DialogTrigger,
  Link,
  makeStyles,
  shorthands,
  tokens,
} from "@fluentui/react-components";
import { DeleteFilled, SignOutFilled } from "@fluentui/react-icons";
import { Form } from "react-router-dom";

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
  warningButton: {
    color: tokens.colorStatusDangerForeground1,
    backgroundColor: tokens.colorStatusDangerBackground1,
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
            <DeleteAccount styles={styles} />
            &nbsp;
            <Logout
              postLogoutRedirect={window.location.href}
              customRenderer={({ href }) => (
                <Button
                  href={href}
                  as="a"
                  appearance="primary"
                  icon={<SignOutFilled />}
                >
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

const DeleteAccount = ({
  styles,
}: {
  styles: ReturnType<typeof useStyles>;
}) => {
  return (
    <Dialog>
      <DialogTrigger>
        <Button as="a" icon={<DeleteFilled />} className={styles.warningButton}>
          Delete Account
        </Button>
      </DialogTrigger>
      <DialogSurface>
        <DialogTitle>Delete Account</DialogTitle>
        <DialogContent>
          Do you want to delete your account? This will render your API Key
          unusable and log you out.
        </DialogContent>
        <DialogActions>
          <DialogTrigger disableButtonEnhancement>
            <Button appearance="primary">Cancel</Button>
          </DialogTrigger>
          <Logout
            postLogoutRedirect={window.location.href}
            customRenderer={({ href }) => (
              <Form method="delete">
                <input type="hidden" value={href} name="redirectUrl" />
                <Button
                  icon={<DeleteFilled />}
                  type="submit"
                  as="button"
                  className={styles.warningButton}
                >
                  Delete Account
                </Button>
              </Form>
            )}
          />
        </DialogActions>
      </DialogSurface>
    </Dialog>
  );
};
