import { useEffect, useState } from 'react';
import { Input, Label } from '@fluentui/react-components';

interface InputProps {
    label: string;
    defaultValue: number;
    onUpdate: (newValue: number) => void;
    type: "text" | "number" | "password" | "search" | "time" | "email" | "tel" | "url" | "date" | "datetime-local" | "month" | "week";
    min: number;
    max: number;
    disabled: boolean;
};

export const ParamInput = (props: InputProps) => {
    const { label, defaultValue, onUpdate, min, max, ...rest } = props;
    const [value, setValue] = useState(defaultValue);

    useEffect(() => {
        setValue(defaultValue);
    }, [defaultValue]);

    return (
        <>
            <Label style={{ fontSize: "medium", marginBottom: "0.5rem", textAlign: "justify" }}>
                <strong>{label}</strong>
            </Label>
            <Input
                onChange={(e) => {
                    const newValue = e.currentTarget.value;
                    if (newValue && (parseFloat(newValue) >= min) && (parseFloat(newValue) <= max)) {
                        setValue(parseFloat(newValue));
                    }
                }}
                onBlur={() => {
                    if (!value) {
                        onUpdate(min);
                    } else {
                        onUpdate(value)}
                    }}
                min={min}
                max={max}
                value={value.toString()}
                {...rest}
            />
            <Label style={{ color: "GrayText", fontSize:"small", textAlign: "justify" }}>
                Accepted Value: {min} - {max}
            </Label>
        </>
    );
};

