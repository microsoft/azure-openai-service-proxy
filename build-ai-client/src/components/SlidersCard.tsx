import { Body1, Card, CardHeader, Input, Label, makeStyles } from "@fluentui/react-components"
import { SliderComponent } from "./SliderComponent";
import { ApiData } from "../interfaces/ApiData";
import {SliderInput} from "./SliderInput";
import { useCallback, useState } from "react";

const useStyles = makeStyles({
        card: {
            height: "100vh",
        }
    })

interface SliderCardProps {
    startSliders: Omit<ApiData, "messages">;
    tokenUpdate: (label: keyof Omit<ApiData, "messages">, newValue: number | string) => void;
    name: string;
    eventUpdate: (eventCode: string) => void;
}

export const SlidersCard =({ startSliders, tokenUpdate, name, eventUpdate }: SliderCardProps) => {
    const sliderCard = useStyles();
    const updateParams = useCallback((label: keyof Omit<ApiData, "messages">) => {
        return (newValue: number | string) => {
            tokenUpdate(label, newValue);
        };
    }, [tokenUpdate]);
    const [eventCode, setEventCode] = useState("");

    return (
        <Card className={sliderCard.card}>
            <CardHeader
                style={{ height: "5vh" }}
                header={
                    <Body1 style={{ fontSize: "large" }}>
                        <b>Parameters</b>
                    </Body1>
                }
            />
            <div style={{ display: "flex", flexDirection: "column", alignItems: "center", height: "1px" }}>
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
            <SliderComponent
                label={"Tokens"}
                defaultValue={startSliders.max_tokens}
                min={0}
                max={4000}
                step={200}
                onUpdate={updateParams("max_tokens")}
            />
            <SliderComponent
                label={"Temperature"}
                defaultValue={startSliders.temperature}
                min={0}
                max={1}
                step={0.1}
                onUpdate={updateParams("temperature")}
            />
            <SliderComponent
                label={"Top P"}
                defaultValue={startSliders.top_p}
                min={0}
                max={1}
                step={0.1}
                onUpdate={updateParams("top_p")}
            />
            <SliderInput
                type="text"
                label={"Stop Sequence"}
                defaultValue={startSliders.stop_sequence}
                onUpdate={updateParams("stop_sequence")}
            />
            <SliderComponent
                label={"Frequency Penalty"}
                defaultValue={startSliders.frequency_penalty}
                min={0}
                max={2}
                step={0.1}
                onUpdate={updateParams("frequency_penalty")}
            />
            <SliderComponent
                label={"Presence Penalty"}
                defaultValue={startSliders.presence_penalty}
                min={0}
                max={2}
                step={0.1}
                onUpdate={updateParams("presence_penalty")}
            />
            <Label
                style={{color: "GrayText", fontSize:"small", textAlign: "center"}}>
                    {name}
                </Label>
            {/* add small text with answer deployment*/}
        </Card>
    );
}