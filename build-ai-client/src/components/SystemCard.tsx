import { Card, Input, Label } from "@fluentui/react-components"
import { Message } from "../interfaces/Message";
import { useState } from "react";

interface SystemProps {
    defaultPrompt: Message;
    onPromptChange: (newPrompt: Message) => void;
}

export const SystemCard =({ defaultPrompt, onPromptChange}: SystemProps) => {

    const [sysPrompt, setPrompt] = useState(defaultPrompt.content)

    return (
        <Card>
            <Label><b>System Message</b></Label>
            <Input
            value={sysPrompt}
            onChange={(event) => {
              setPrompt(event.target.value);
            }}
            onKeyDown={(event) => {
              if (event.key === "Enter") {
                onPromptChange({role: "system", content: sysPrompt});
              }
            }}
            ></Input>
        </Card>
    )
}