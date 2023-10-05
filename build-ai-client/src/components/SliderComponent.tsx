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

  const [value, setValue] = useState(defaultValue);
  const [sliderValue, setSliderValue] = useState(defaultValue);

  const onSliderChange = (data: string) => {

      setSliderValue(parseFloat(data));
      onUpdate(parseFloat(data))

  }

  return (
    <>
      <Label htmlFor={mediumId}>{label}</Label>
      <Input
      type = "number"
      onBlur={(event) => {
        let numData = parseFloat(event.target.value);
        if (numData > max || numData < min) {
          console.error("Value out of range");
          return;
        }
        else{
          setValue(parseFloat(event.target.value));
          onUpdate(parseFloat(event.target.value))
        }
      }}></Input>
      <Slider
        size="medium"
        value={sliderValue}
        onChange={() => {setSliderValue(value); onUpdate(value)}}
      />
      <Label htmlFor={mediumId}>Current Value: {value}</Label>
    </>
  );
};
