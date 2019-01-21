import { MSCords } from "./structure/mscords"
import { MSGrid } from "./structure/msgrid"
import { MSRoom, MSRoomConf } from "./structure/msroom"

/**
 * 이벤트 목록
 */
export enum MdEvents {
    chat = "chated",
    conf = "confed",
    connect = "connected",
    enter = "entered",
    error = "error",
    leave = "left",
    move = "moved",
    start = "started",
}
export interface MdCommands {
    connect:{
        invite:string,
    };
    chat:{
        content:string,
    };
    conf:{
        conf:MSRoomConf,
    };
    ban:{};
    start:{};
    move:{
        /**
         * @type 좌표
         */
        start:MSCords,
        /**
         * @type 좌표
         */
        end:MSCords,
        /**
         * @type 벡터 (Delta)
         */
        dir:MSCords,
    };
    gg:{};
}
/**
 * 연결됐을 때
 */
export interface ConnectInfo extends TypedInfo<MdEvents.connect> {
    room:MSRoom,
}
/**
 * 채팅이 왔을 때
 */
export interface ChatInfo extends EuserInfo<MdEvents.chat> {
    content:string,
}
/**
 * 설정이 변경됐을 때
 */
export interface ConfInfo extends TypedInfo<MdEvents.conf> {
    conf:MSRoomConf,
}
/**
 * 게임이 시작됐을 때
 */
export interface StartInfo extends TypedInfo<MdEvents.start> {
    black:number,
    white:number,
    board:MSGrid,
    turn:"black" | "white"
}
/**
 * 누군가 나갔을 때
 */
export interface LeaveInfo extends EuserInfo<MdEvents.leave> {}
/**
 * 유저가 들어왔을 때
 */
export interface EnterInfo extends EuserInfo<MdEvents.enter> {
    room:MSRoom,
}
/**
 * 에러날 때
 */
export interface ErrorInfo extends TypedInfo<MdEvents.error> {
    msg:string,
}

/*
    도움말들
*/
/**
 * type을 가진 정보
 */
interface TypedInfo<T extends MdEvents> {
    type:T,
}
/**
 * user ID를 가진 정보
 */
interface EuserInfo<T extends MdEvents> extends TypedInfo<T> {
    user:number,
}
export type MdEventTypes = TypedInfo<MdEvents>