import React, { useState } from 'react';
import { Input, Label } from '@fluentui/react-components';

interface InputProps {
    label: string;
    defaultValue: number | string;
    onUpdate: (newValue: number | string) => void;
    type: "text" | "number" | "password" | "search" | "time" | "email" | "tel" | "url" | "date" | "datetime-local" | "month" | "week";
    min?: number;
    max?: number;
};

export const ParamInput = ({ label, defaultValue, onUpdate, type, min, max }: InputProps) => {

    const [value, setValue] = useState(defaultValue.toString());

    return (
        <>
            <Label style={{ fontSize: "medium", marginBottom: "0.5rem" }}>
                <b>{label}</b>
            </Label>
            <Input
                type={type}
                placeholder={value.toString()}
                onChange={(e) => {
                    const newValue = e.currentTarget.value;
                    if ((min === undefined || parseFloat(newValue) >= min) && (max === undefined || parseFloat(newValue) <= max)) {
                        setValue(newValue);
                    }
                }}
                onBlur={() => onUpdate(value)}
                style={{ textAlign: "center" }}
                min={min}
                max={max}
                value={value}
            />
        </>
    );
};

