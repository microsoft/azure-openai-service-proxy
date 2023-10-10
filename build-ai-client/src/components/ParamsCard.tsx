import { Body1, Card, CardFooter, CardHeader, Input, Label, makeStyles } from "@fluentui/react-components"
import { ApiData } from "../interfaces/ApiData";
import {ParamInput} from "./ParamInput";
import { useCallback, useState } from "react";

const useStyles = makeStyles({
        card: {
            marginTop: "10px",
            marginRight: "10px",
            marginBottom: "10px",
            marginLeft: "10px",
        },
        divider: {
            display: "flex", 
            flexDirection: "column", 
            alignItems: "center", 
            height: "1px"
        }
    })

interface ParamsCardProps {
    startValues: Omit<ApiData, "messages">;
    tokenUpdate: (label: keyof Omit<ApiData, "messages">, newValue: number | string) => void;
    name: string;
    eventUpdate: (eventCode: string) => void;
}

export const ParamsCard =({ startValues, tokenUpdate, name, eventUpdate }: ParamsCardProps) => {
    const styles = useStyles();
    const updateParams = useCallback((label: keyof Omit<ApiData, "messages">) => {
        return (newValue: number | string) => {
            tokenUpdate(label, newValue);
        };
    }, [tokenUpdate]);
    const [eventCode, setEventCode] = useState("");

    return (
        <Card className={styles.card}>
            <CardHeader
                style={{ height: "10vh" }}
                header={
                    <Body1 style={{ fontSize: "large" }}>
                        <h2>Parameters</h2>
                    </Body1>
                }
            />
            <div className={styles.divider}>
                <Label style={{ fontSize: "medium", marginBottom: "0.5rem" }}>
                    <b>Event Code</b>
                </Label>
                <Input
                    type="password"
                    placeholder={"Enter your Event Code"}
                    onChange={(e) => {
                        setEventCode(e.target.value);
                    }}
                    onBlur={() => eventUpdate(eventCode)}
                    style={{ textAlign: "center" }}
                />
            </div>
            <div className={styles.divider}>
                <ParamInput 
                label={"Tokens"}
                defaultValue={startValues.max_tokens}
                onUpdate={updateParams("max_tokens")}
                type={"number"}
                min={1}
                max={4000}
                />
            </div>
            <div className={styles.divider}>
                <ParamInput 
                label={"Temperature"}
                defaultValue={startValues.temperature}
                onUpdate={updateParams("temperature")}
                type={"number"} 
                min={0}
                max={1}
                />
            </div>
            <div className={styles.divider}>
                <ParamInput 
                label={"Top P"}
                defaultValue={startValues.top_p}
                onUpdate={updateParams("top_p")}
                type={"number"}
                min={0}
                max={1}
                />
            </div>
            <CardFooter style={{ height: "10vh" }}>
                <Label
                    style={{color: "GrayText", fontSize:"small", textAlign: "center"}}>
                        {name}
                </Label>
            </CardFooter>
        </Card>
    );
}