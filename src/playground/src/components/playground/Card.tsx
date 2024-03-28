import {
  Body1,
  CardHeader as FluentUICardHeader,
  CardHeaderProps,
  CardProps,
  Card as FluentUICard,
  makeStyles,
  mergeClasses,
} from "@fluentui/react-components";

const useStyles = makeStyles({
  card: {
    paddingTop: "0px",
    paddingBottom: "24px",
    paddingRight: "24px",
    paddingLeft: "24px",
    marginTop: "24px",
    textAlign: "left"
  },
  header: {
    marginTop: "0px",
    marginBottom: "12px",
    marginRight: "0px",
    marginLeft: "0px",
    paddingBottom: "0px",
    paddingTop: "0px",
    textAlign: "left",
    fontWeight: "bold",
    borderBottomColor: "#306ab7",
    borderBottomStyle: "solid",
    borderBottomWidth: "2px",
    color: "#111"
  },
  maxWidth: {
    maxWidth: "100%",
  },
});

export const CardHeader = ({
  header,
  ...rest
}: { header: string } & CardHeaderProps) => {
  const styles = useStyles();
  return (
    <FluentUICardHeader
      {...rest}
      className={styles.header}
      header={
        <div className={styles.maxWidth}>
          <Body1>
            <h2>{header}</h2>
          </Body1>
        </div>
      }
    />
  );
};

export const Card = ({
  children,
  className,
  header,
  ...rest
}: CardProps & { header?: string }) => {
  const styles = useStyles();

  return (
    <FluentUICard className={mergeClasses(styles.card, className)} {...rest}>
      {header && <CardHeader className={styles.header} header={header} />}
      {children}
    </FluentUICard>
  );
};
