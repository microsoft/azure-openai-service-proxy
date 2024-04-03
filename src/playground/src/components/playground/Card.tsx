import {
  Body1,
  CardHeader as FluentUICardHeader,
  CardHeaderProps,
  CardProps,
  Card as FluentUICard,
  makeStyles,
  mergeClasses,
  shorthands,
} from "@fluentui/react-components";

const useStyles = makeStyles({
  card: {
    ...shorthands.padding("0px", "24px", "24px"),
    marginTop: "24px",
    textAlign: "left"
  },
  header: {
    ...shorthands.margin("0px", "0px", "12px"),
    ...shorthands.padding("0px"),
    textAlign: "left",
    fontWeight: "bold",
    ...shorthands.borderBottom("2px", "solid", "#306ab7"),
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
