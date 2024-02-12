import {
  Button,
  Dialog,
  DialogActions,
  DialogBody,
  DialogContent,
  DialogSurface,
  DialogTitle,
  DialogTrigger,
} from "@fluentui/react-components";
import {
  AuthStatus,
  useEventDataContext,
} from "../../providers/EventDataProvider";

export const Unauthorised = () => {
  const { authStatus, setEventCode: setEventConnection } =
    useEventDataContext();

  if (authStatus !== AuthStatus.NotAuthorized) {
    return null;
  }

  return (
    <Dialog open={true}>
      <DialogSurface>
        <DialogBody>
          <DialogTitle>Invalid Event Code</DialogTitle>
          <DialogContent>
            The API Key provided is invalid or the event has expired. Please check the API Key and the event start and end times.
          </DialogContent>
          <DialogActions>
            <DialogTrigger disableButtonEnhancement>
              <Button
                appearance="primary"
                onClick={() => setEventConnection("")}
              >
                Close
              </Button>
            </DialogTrigger>
          </DialogActions>
        </DialogBody>
      </DialogSurface>
    </Dialog>
  );
};
