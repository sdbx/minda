export interface Connect {
    type:"connect"
    invite:string
}

export interface Chat {
    type:"chat"
    content:string
}

export type Command = Connect | Chat