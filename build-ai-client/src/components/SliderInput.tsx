import React, { useState } from 'react';
import { Input, Label } from '@fluentui/react-components';

interface InputProps {
    label: string;
    defaultValue: number | string;
    onUpdate: (newValue: number | string) => void;
    type: "text" | "number" | "password" | "search" | "time" | "email" | "tel" | "url" | "date" | "datetime-local" | "month" | "week";
};

export const SliderInput = ({ label, defaultValue, onUpdate, type }: InputProps) => {

    const [value, setValue] = useState(defaultValue.toString());

    return (
        <div style={{ display: "flex", alignItems: "center", height: "1px" }}>
            <Label style={{fontSize: "medium"}}><b>{label}</b></Label>
            <Input type={type} placeholder={value} onChange={(event, data) => setValue(data?.value || "")} onBlur={() => onUpdate(value)} />
        </div>
    );
};

