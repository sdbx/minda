import { Room, RoomConf } from "./mdmodel"

export interface Connected {
    type:"connected"
    room:Room
}

export interface Chated {
    type:"chated"
    user:number
    content:string
}

export interface Confed {
    type:"confed"
    conf:RoomConf
}

export type Event = Connected | Chated