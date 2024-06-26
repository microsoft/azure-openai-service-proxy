import { useEffect, useState } from "react";
import { Input, useId, makeStyles, shorthands } from "@fluentui/react-components";
import { LabelWithTooltip } from "./LabelWithTooltip";

interface InputProps {
  label: string;
  defaultValue: number;
  onUpdate: (newValue: number) => void;
  type:
    | "text"
    | "number"
    | "password"
    | "search"
    | "time"
    | "email"
    | "tel"
    | "url"
    | "date"
    | "datetime-local"
    | "month"
    | "week";
  min: number;
  max: number;
  disabled: boolean;
  explain: string;
}

const useStyles = makeStyles({
  input: {
    fontSize: "14px",
    ...shorthands.margin("10px", "0px", "0px"),
    width: "100%",
    textAlign: "left",
    ...shorthands.padding("1px", "6px", "6px", "10px"),
    height: "auto",
  },
  container: {
    marginTop: "0px",
  },
});

export const ParamInput = (props: InputProps) => {
  const { label, explain, defaultValue, onUpdate, min, max, ...rest } = props;
  const [value, setValue] = useState(defaultValue);
  const id = useId();
  const styles = useStyles();

  useEffect(() => {
    setValue(defaultValue);
  }, [defaultValue]);

  return (
    <div className={styles.container}>
      <LabelWithTooltip label={label} explain={explain} id={id} />

      <Input
        className={styles.input}
        id={id}
        onChange={(e) => {
          const newValue = e.currentTarget.value;
          if (newValue === "" || newValue === undefined) {
            setValue(0);
          } else {
            if (
              newValue &&
              parseFloat(newValue) >= min &&
              parseFloat(newValue) <= max
            ) {
              setValue(parseFloat(newValue));
            }
          }
        }}
        onBlur={() => {
          if (!value) {
            onUpdate(min);
          } else {
            onUpdate(value);
          }
        }}
        min={min}
        max={max}
        value={value.toString()}
        {...rest}
      />
      <div
        style={{
          color: "GrayText",
          fontSize: "small",
          textAlign: "left",
          margin: "0px",
        }}
      >
        Accepted Value: {min} - {max}
      </div>
    </div>
  );
};
