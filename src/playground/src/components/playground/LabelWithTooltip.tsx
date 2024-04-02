import { Label, Tooltip, makeStyles } from "@fluentui/react-components";
import { Info16Filled } from "@fluentui/react-icons";

const useStyles = makeStyles({
  label: {
    fontSize: "medium",
    marginTop: "0px",
    marginBottom: "0px",
    textAlign: "justify",
    display: "block",
    fontWeight: "bold",
  },
  tooltip: {
    marginLeft: "6px",
  },
});

export type LabelWithTooltipProps = {
  label: string;
  id: string;
  explain: string;
};

export const LabelWithTooltip = ({
  label,
  id,
  explain,
}: LabelWithTooltipProps) => {
  const styles = useStyles();
  return (
    <Label className={styles.label} htmlFor={id}>
      {label}
      <Tooltip content={explain} relationship="description">
        <Info16Filled className={styles.tooltip} />
      </Tooltip>
    </Label>
  );
};
