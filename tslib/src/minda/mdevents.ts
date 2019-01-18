import { MSGrid } from "./structure/msgrid"
import { MSRoom, MSRoomConf } from "./structure/msroom"

export interface ConnectInfo extends TypedInfo<MdEvents.connect> {
    room:MSRoom,
}
export interface ConfInfo extends TypedInfo<MdEvents.conf> {
    conf:MSRoomConf,
}
export interface ChatInfo extends EuserInfo<MdEvents.chat> {
    content:string,
}
export interface StartInfo extends TypedInfo<MdEvents.start> {
    black:number,
    white:number,
    board:MSGrid,
    turn:"black" | "white"
}
export interface LeaveInfo extends EuserInfo<MdEvents.leave> {

}
// tslint:disable-next-line
export interface EnterInfo extends EuserInfo<MdEvents.enter> {
    room:MSRoom,
}
// tslint:disable-next-line
interface TypedInfo<T extends MdEvents> {
    type:T,
}
interface EuserInfo<T extends MdEvents> extends TypedInfo<T> {
    user:number,
}

export interface Confed {
    conf:MSRoomConf
}
export type MdEventTypes = 
    ConnectInfo | ChatInfo | EnterInfo | ConfInfo
    | StartInfo | LeaveInfo
export enum MdEvents {
    chat = "chated",
    enter = "entered",
    connect = "connected",
    conf = "confed",
    start = "started",
    leave = "left",
}