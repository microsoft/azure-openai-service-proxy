import {
  CardProps,
  Card as FluentUICard,
  makeStyles,
  mergeClasses,
} from "@fluentui/react-components";

const useStyles = makeStyles({
  card: {
    marginTop: "10px",
    marginRight: "10px",
    marginBottom: "10px",
    marginLeft: "10px",
  },
});

export const Card = ({ children, className, ...rest }: CardProps) => {
  const styles = useStyles();
  return (
    <FluentUICard className={mergeClasses(styles.card, className)} {...rest}>
      {children}
    </FluentUICard>
  );
};
