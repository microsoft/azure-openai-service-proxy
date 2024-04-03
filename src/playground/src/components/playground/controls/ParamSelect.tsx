import {
  Select,
  makeStyles,
  shorthands,
  useId,
} from "@fluentui/react-components";
import { LabelWithTooltip, LabelWithTooltipProps } from "./LabelWithTooltip";

const useStyles = makeStyles({
  input: {
    fontSize: "medium",
    marginLeft: "0px",
    width: "100%",
    textAlign: "left",
    height: "auto",
    ...shorthands.margin("0px", "0px", "0px", "0px"),
  },
  label: {
    fontSize: "medium",
    marginBottom: "0px",
    marginTop: "0px",
    textAlign: "justify",
    display: "block",
    fontWeight: "bold",
  },
});

type Props = {
  onUpdate: (newValue: string) => void;
  disabled: boolean;
  defaultOption?: string;
  options: string[];
  defaultValue?: string | number;
} & Omit<LabelWithTooltipProps, "id">;

export const ParamSelect = ({
  label,
  explain,
  onUpdate,
  disabled,
  defaultOption,
  options,
  defaultValue,
}: Props) => {
  const styles = useStyles();
  const id = useId();
  return (
    <>
      <LabelWithTooltip label={label} explain={explain} id={id} />

      <Select
        id={id}
        className={styles.input}
        disabled={disabled}
        onChange={(e) => {
          const newValue = e.currentTarget.value;
          onUpdate(newValue);
        }}
        defaultValue={defaultValue}
      >
        {defaultOption && <option value="">{defaultOption}</option>}
        {options.map((o) => (
          <option key={o} value={o}>
            {o}
          </option>
        ))}
      </Select>
    </>
  );
};
