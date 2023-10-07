import React, { useState } from 'react';
import { Input, Label } from '@fluentui/react-components';

interface InputProps {
    label: string;
    defaultValue: number | string;
    onUpdate: ( newValue: number | string) => void;
};

export const SliderInput = ({ label, defaultValue, onUpdate }: InputProps) => {

    const [value, setValue] = useState(defaultValue.toString());

    return (
        <div>
            <Label style={{fontSize: "large"}}><b>{label}</b></Label>
            <Input type="text" placeholder={value} onChange={() => setValue(value)} onBlur={() => onUpdate} />
        </div>
    );
};


