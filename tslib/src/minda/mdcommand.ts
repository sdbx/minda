import { RoomConf } from "./mdmodel"

export interface Connect {
    type:"connect"
    invite:string
}

export interface Chat {
    type:"chat"
    content:string
}

export interface Conf {
    type:"conf"
    conf:RoomConf
}

export type Command = Connect | Chat