import * as React from "react";
import { useId, Label, Slider, Input } from "@fluentui/react-components";
import { useState } from "react";
import type {SliderProps} from "@fluentui/react-components"
import { ApiData } from "../interfaces/ApiData";

interface SliderComponentProps {
  label: string;
  defaultValue: number;
  min: number;
  max: number;
  step: number;
  onUpdate: ( newValue: number | string) => void;
}

export const SliderComponent = ({
  label,
  defaultValue,
  min,
  max,
  step,
  onUpdate
}: SliderComponentProps) => {

  const mediumId = useId("medium");

  // const [value, setValue] = useState(defaultValue);

  const id = useId();
  const [sliderValue, setSliderValue] = useState(defaultValue);

  const onSliderChange: SliderProps["onChange"] = (_, data) =>{
    setSliderValue(data.value);
  }

  React.useEffect(() => {
    onUpdate(sliderValue)
  }, [sliderValue])

  return (
    <>
      <Label htmlFor={mediumId}><b>{label}</b></Label>
      <Input
        id={mediumId}
        type="number"
        placeholder={sliderValue.toString()}
        onChange={(event)=> {
          setSliderValue(parseFloat(event.target.value))
        }}
        onBlur={() => onSliderChange}
      />
      <Label htmlFor={mediumId}>
        Control Slider [ Current Value: {sliderValue} ]
      </Label>
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
