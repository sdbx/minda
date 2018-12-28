import { Room } from "./mdmodel"

export interface Connected {
    type:"connected"
    room:Room
}

export interface Chated {
    type:"chated"
    user:number
    content:string
}

export type Event = Connected | Chated