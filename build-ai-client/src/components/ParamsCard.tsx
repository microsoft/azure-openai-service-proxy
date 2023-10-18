import { Body1, Button, Card, CardFooter, CardHeader, Divider, Input, Label, makeStyles } from "@fluentui/react-components"
import { ApiData } from "../interfaces/ApiData";
import {ParamInput} from "./ParamInput";
import { useCallback, useState } from "react";
import { UsageData } from "../interfaces/UsageData";
import { useEventDataContext } from "../EventDataProvider";

const useStyles = makeStyles({
        card: {
            marginTop: "10px",
            marginRight: "10px",
            marginBottom: "10px",
            marginLeft: "10px",
        },
        dividerblock: {
            display: "flex", 
            flexDirection: "column", 
            alignItems: "right", 
            justifyContent: "center"
        },
        smallbutton: {
            width: "100%",
            height: "50%",
            maxWidth: "none",
            maxHeight: "25%",
            backgroundColor: "#f2f2f2"
        },
        dividerline: {
            maxHeight: "1%"
        }
    })

interface ParamsCardProps {
    startValues: Omit<ApiData, "messages">;
    tokenUpdate: (label: keyof Omit<ApiData, "messages">, newValue: number | string) => void;
    name: string;
    usageData: UsageData;
}

export const ParamsCard =({ startValues, tokenUpdate, name, usageData }: ParamsCardProps) => {
    const styles = useStyles();
    const updateParams = useCallback((label: keyof Omit<ApiData, "messages">) => {
        return (newValue: number | string) => {
            tokenUpdate(label, newValue);
        };
    }, [tokenUpdate]);
    const [code, setCode] = useState("");
    const { eventData, setEventCode, isAuthorized } = useEventDataContext();
    const maxTokens = eventData?.max_token_cap ?? 0;

    return (
        <Card className={styles.card}>
            <CardHeader
                style={{ height: "10vh", alignItems: "start"}}
                header={
                    <Body1 style={{ fontSize: "large" }}>
                        <h2>Configuration</h2>
                    </Body1>
                }
            />
            <div className={styles.dividerblock}>
                <Label style={{ fontSize: "medium", marginBottom: "0.5rem", textAlign: "justify"}}>
                    <b>Event Code</b>
                </Label>
                <Input
                type="password" 
                placeholder="Enter your Event Code"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                style={{ textAlign: "right" }}/>

                {!isAuthorized && (
                    <>
                        <Button
                            className={styles.smallbutton}
                            onClick={() => setEventCode(code)}
                        >
                            Log In
                        </Button>
                        <Label style={{ color: "GrayText", fontSize: "small", textAlign: "justify" }}>
                            Provided by workshop host.
                        </Label>
                    </>
                )}
                {isAuthorized && (
                    <Label style={{ color: "GrayText", fontSize: "small", textAlign: "justify" }}>
                        <div>{eventData!.name}</div>
                        <div>
                            <a href={eventData!.url} target="_blank" rel="noopener noreferrer">
                                {eventData!.url_text}
                            </a>
                        </div>
                    </Label>
                )}
            </div>
            <Divider className={styles.dividerline}></Divider>
            <div className={styles.dividerblock}>
                <ParamInput 
                label="Tokens"
                defaultValue={maxTokens / 2}
                onUpdate={updateParams("max_tokens")}
                type="number"
                min={1}
                max={maxTokens}
                disabled={!isAuthorized}
                />
            </div>
            <Divider className={styles.dividerline} ></Divider>
            <div className={styles.dividerblock}>
                <ParamInput 
                label="Temperature"
                defaultValue={startValues.temperature}
                onUpdate={updateParams("temperature")}
                type="number" 
                min={0}
                max={1}
                disabled={!isAuthorized}
                />
            </div>
            <Divider className={styles.dividerline} ></Divider>
            <div className={styles.dividerblock}>
                <ParamInput 
                label="Top P"
                defaultValue={startValues.top_p}
                onUpdate={updateParams("top_p")}
                type="number"
                min={0}
                max={1}
                disabled={!isAuthorized}
                />
            </div>
            <Divider className={styles.dividerline} ></Divider>
            <div className={styles.dividerblock}>
                <Label style={{color: "GrayText", fontSize:"small" ,textAlign: "justify"}}>
                    <div>Finish Reason: {usageData.finish_reason}</div>
                    <div>Completion Tokens: {usageData.completion_tokens}</div>
                    <div>Prompt Tokens: {usageData.prompt_tokens}</div>
                    <div>Total Tokens: {usageData.total_tokens}</div>
                    <div>Response Time: {usageData.response_time} ms</div>
                </Label>
            </div>
            <Divider className={styles.dividerline} ></Divider>
            <CardFooter style={{ height: "5vh" }}>
                <Label
                    style={{color: "GrayText", fontSize:"small", textAlign: "center"}}>
                        {name}
                </Label>
            </CardFooter>
        </Card>
    );
}