import { Label, makeStyles } from "@fluentui/react-components";

const useStyles = makeStyles({
  label: {
    fontSize: "medium",
    marginBottom: "0.5rem",
    textAlign: "justify",
  },
});

export const ParamInputLabel = ({
  label,
  id,
}: {
  label: string;
  id: string;
}) => {
  const sytles = useStyles();
  return (
    <Label className={sytles.label} htmlFor={id}>
      <strong>{label}</strong>
    </Label>
  );
};
