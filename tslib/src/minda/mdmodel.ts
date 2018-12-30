export interface Room {
    id:string,
    conf:RoomConf
}

export interface RoomConf {
    name:string,
    black:number,
    white:number,
    king:number
}