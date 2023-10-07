import { Body1, Button, Card, CardHeader, Textarea, makeStyles } from "@fluentui/react-components"
import { MessageData } from "../interfaces/MessageData";
import { useState } from "react";
import { Save24Regular } from "@fluentui/react-icons";

interface SystemProps {
    defaultPrompt: MessageData;
    onPromptChange: (newPrompt: MessageData) => void;
}

const useStyles = makeStyles({
    wrapper: {
        display: "flex",
        flexDirection: "column",
        rowGap: "15px",
    }

})

export const SystemCard =({ defaultPrompt, onPromptChange}: SystemProps) => {

    const [sysPrompt, setPrompt] = useState(defaultPrompt.content)
    const styles = useStyles();
    return (
        <Card>
            <CardHeader
                style={{height: "5vh"}}
                header={
                <Body1 style={{fontSize: "large"}}>
                <b>System Message</b>
                </Body1>
                }
                />
            <div>
                <Textarea
                className="test"
                style={{height: "12.5%", maxHeight: "30%", width: "100%" }}
                value={sysPrompt}
                onChange={(event) => {
                setPrompt(event.target.value);}}
                onKeyDown={(event) => {
                if (event.key === "Enter") {
                    onPromptChange({role: "system", content: sysPrompt});}}} 
                />
                <div className={styles.wrapper} style={{padding: "15px"}}>                
                    <Button
                    icon={<Save24Regular />} iconPosition="after"
                    onClick={() => {
                        onPromptChange({role: "system", content: sysPrompt});
                    }}>
                        Save Changes
                    </Button>
                </div>

            </div>     
        </Card>
    )
}