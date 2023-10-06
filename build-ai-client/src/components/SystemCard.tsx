import { Body1, Card, CardHeader, Input } from "@fluentui/react-components"
import { MessageData } from "../interfaces/MessageData";
import { useState } from "react";

interface SystemProps {
    defaultPrompt: MessageData;
    onPromptChange: (newPrompt: MessageData) => void;
}

export const SystemCard =({ defaultPrompt, onPromptChange}: SystemProps) => {

    const [sysPrompt, setPrompt] = useState(defaultPrompt.content)

    return (
        <Card>
            <CardHeader
                style={{height: "10vh"}}
                header={
                <Body1 style={{fontSize: "large"}}>
                <b>System Message</b>
                </Body1>
                }/>
            <div>
                <Input
                style={{width: "100%", height: "10vh", fontSize: "large"}}
                value={sysPrompt}
                onChange={(event) => {
                setPrompt(event.target.value);}}
                onKeyDown={(event) => {
                if (event.key === "Enter") {
                    onPromptChange({role: "system", content: sysPrompt});
                }}}></Input>
            </div>     
        </Card>
    )
}