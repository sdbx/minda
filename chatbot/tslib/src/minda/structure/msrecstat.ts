import { MSGridEncode } from "./msgrid"
import { MSLoseCause } from "./mslosecause"

export interface MSRecStat {
    black:number;
    white:number;
    loser:number;
    cause:MSLoseCause;
    moves:Array<unknown>;
    created_at:number;
    map:MSGridEncode;
}