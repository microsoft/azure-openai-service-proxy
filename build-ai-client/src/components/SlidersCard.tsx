import { Card, makeStyles, shorthands } from "@fluentui/react-components"
import { SliderComponent } from "./SliderComponent";
import { ApiData } from "../interfaces/ApiData";

const useStyles = makeStyles({
        card: {
            ...shorthands.margin("auto"),
            height: "100vh",
            width: "20vh",
        }
    })

interface SliderCardProps {
    startSliders: Omit<ApiData, "messages">;
    tokenUpdate: (label: keyof Omit<ApiData, "messages">, newValue: number | string) => void;
}


export const SlidersCard =({ startSliders, tokenUpdate }: SliderCardProps) => {
    const sliderCard = useStyles();
    const updateParams = (label: keyof Omit<ApiData, "messages">) => {
        return (newValue: number | string) => {
            tokenUpdate(label, newValue)
        }
    }
    return (
        <Card className={sliderCard.card}>
            <SliderComponent label={"Tokens"} defaultValue={startSliders.max_tokens} min={0} 
                max={4000} step={1000} onUpdate={updateParams("max_tokens")}/>
            <SliderComponent label={"Temperature"} defaultValue={startSliders.temperature} min={0}
                max={1} step={1} onUpdate={updateParams("temperature")}/>
            <SliderComponent label={"Top P"} defaultValue={startSliders.top_p} min={0} 
                max={1} step={1} onUpdate={updateParams("top_p")} />
        </Card>
    )
}