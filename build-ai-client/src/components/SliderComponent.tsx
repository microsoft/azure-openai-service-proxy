import * as React from "react";
import { useId, Label, Slider, Input, makeStyles } from "@fluentui/react-components";
import { useState } from "react";
import type {SliderProps} from "@fluentui/react-components"

interface SliderComponentProps {
  label: string;
  defaultValue: number;
  min: number;
  max: number;
  step: number;
  onUpdate: ( newValue: number | string) => void;
}

const useStyles = makeStyles({
  sliderInput: {
    maxWidth: "65px",
    marginLeft: "10px",
    alignSelf: "end",
    maxHeight: "10px",
  }
});

export const SliderComponent = ({
  label,
  defaultValue,
  min,
  max,
  step,
  onUpdate
}: SliderComponentProps) => {

  const mediumId = useId("medium");
  const id = useId();
  const [sliderValue, setSliderValue] = useState(defaultValue);


  const onSliderChange: SliderProps["onChange"] = (_, data) =>{
    setSliderValue(data.value);
    onUpdate(data.value);
  }


  // useEffect(() => {
  //   console.log("test2");
  //   onUpdate(sliderValue)
  // }, [sliderValue, onUpdate])

  const styles = useStyles();
  return (

    <>
      <div>
      <Label htmlFor={mediumId} style={{textAlign: "left", fontSize: "large"}}><b>{label}</b></Label>
      <Input
        className={styles.sliderInput}
        type="number"
        placeholder={sliderValue.toString()}
        onChange={(event)=> {
          setSliderValue(parseFloat(event.target.value))
        }}
        onBlur={() => onSliderChange}
      />
      </div>
      {/* <Label htmlFor={mediumId}>
        Control Slider [ Current Value: {sliderValue} ]
      </Label> */}
      <Slider
        aria-valuetext={`Value is ${sliderValue}`}
        value={sliderValue}
        min={min}
        max={max}
        step={step}
        onChange={onSliderChange}
        id={id}
      />
    </>
  );
};
