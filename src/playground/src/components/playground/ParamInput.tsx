import { useEffect, useState } from "react";
import { Input, Label, useId, makeStyles, Tooltip } from "@fluentui/react-components";
import { Info16Filled } from "@fluentui/react-icons";

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
    marginLeft: "0px",
    marginBottom: "0px",
    marginTop: "10px",
    width: "100%",
    textAlign: "left",
    paddingLeft: "10px",
    paddingRight: "6px",
    paddingTop: "1px",
    paddingBottom: "6px",
    height: "auto",
  },
  container: {
    marginTop: "0px",
  },
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
      <Label className={styles.label} htmlFor={id}>
        {label}
        <Tooltip content={explain} relationship="description" >
          <Info16Filled className={styles.tooltip} />
        </Tooltip>
      </Label>

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
      <div style={{ color: "GrayText", fontSize: "small", textAlign: "left", margin: "0px" }} >
        Accepted Value: {min} - {max}
      </div>
    </div>
  );
};
