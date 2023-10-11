import { Body1, Button, Card, CardFooter, CardHeader, Divider, Input, Label, makeStyles } from "@fluentui/react-components"
import { ApiData } from "../interfaces/ApiData";
import {ParamInput} from "./ParamInput";
import { useCallback, useState } from "react";
import { EventData } from "../interfaces/EventData";
import { UsageData } from "../interfaces/UsageData";
import { eventInfo } from "../api/eventInfo";

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
    eventUpdate: (eventCode: string) => void;
    usageData: UsageData;
    maxTokens: number;
    eventData: EventData;
}

export const ParamsCard =({ startValues, tokenUpdate, name, eventUpdate, usageData, maxTokens, eventData}: ParamsCardProps) => {
    const styles = useStyles();
    const updateParams = useCallback((label: keyof Omit<ApiData, "messages">) => {
        return (newValue: number | string) => {
            tokenUpdate(label, newValue);
        };
    }, [tokenUpdate]);
    const [eventCode, setEventCode] = useState("");
    const [isCodeSubmitted, setIsCodeSubmitted] = useState(false);
    const [eData, setEData] = useState(eventData);

    const getEventData = async () => {
        const data = await eventInfo(eventCode);
        setEData(data);
      }

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
                placeholder={"Enter your Event Code"}
                value={eventCode}
                onChange={(e) => {
                    setEventCode(e.target.value);
                }}
                style={{ textAlign: "right" }}/>
                
                {!isCodeSubmitted && (
                    <>
                        <Button
                            className={styles.smallbutton}
                            onClick={() => {
                                eventUpdate(eventCode);
                                getEventData();
                                if(eData.authorized === false) alert("Invalid Event Code");
                                else setIsCodeSubmitted(true);
                            }}
                        >
                            Log In
                        </Button>
                        <Label style={{ color: "GrayText", fontSize: "small", textAlign: "justify" }}>
                            Provided by workshop host.
                        </Label>
                    </>
                )}
                {isCodeSubmitted && (
                    <Label style={{ color: "GrayText", fontSize: "small", textAlign: "justify" }}>
                        <div>Welcome to {eData.event_name}!</div>
                        <div>
                            <a href={eData.event_url} target="_blank" rel="noopener noreferrer">
                                {eData.event_url_text}
                            </a>
                        </div>
                    </Label>
                )}
            </div>
            <Divider className={styles.dividerline} ></Divider>
            <div className={styles.dividerblock}>
                <ParamInput 
                label={"Tokens"}
                defaultValue={1}
                onUpdate={updateParams("max_tokens")}
                type={"number"}
                min={1}
                max={maxTokens}
                />
            </div>
            <Divider className={styles.dividerline} ></Divider>
            <div className={styles.dividerblock}>
                <ParamInput 
                label={"Temperature"}
                defaultValue={startValues.temperature}
                onUpdate={updateParams("temperature")}
                type={"number"} 
                min={0}
                max={1}
                />
            </div>
            <Divider className={styles.dividerline} ></Divider>
            <div className={styles.dividerblock}>
                <ParamInput 
                label={"Top P"}
                defaultValue={startValues.top_p}
                onUpdate={updateParams("top_p")}
                type={"number"}
                min={0}
                max={1}
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