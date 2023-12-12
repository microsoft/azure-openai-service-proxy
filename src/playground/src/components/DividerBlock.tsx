import { Divider, makeStyles } from "@fluentui/react-components";
import { PropsWithChildren } from "react";
const useStyles = makeStyles({
  dividerblock: {
    display: "flex",
    flexDirection: "column",
    alignItems: "right",
    justifyContent: "center",
  },
  dividerline: {
    maxHeight: "1%",
  },
});

export const DividerBlock = ({ children }: PropsWithChildren) => {
  const styles = useStyles();
  return (
    <>
      <div className={styles.dividerblock}>{children}</div>
      <Divider className={styles.dividerline}></Divider>
    </>
  );
};
