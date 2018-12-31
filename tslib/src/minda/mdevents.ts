import { MSRoom, MSRoomConf } from "./structure/msroom"

export interface ConnectInfo extends TypedInfo<MdEvents.connect> {
    room:MSRoom,
}
export interface ChatInfo extends EuserInfo<MdEvents.chat> {
    content:string,
}
// tslint:disable-next-line
export interface EnterInfo extends EuserInfo<MdEvents.enter> {

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
export type MdEventTypes = ConnectInfo | ChatInfo | EnterInfo
export enum MdEvents {
    chat = "chated",
    enter = "entered",
    connect = "connected",
}