import { MSCords } from "./mscords"
import { MSGameRule } from "./msgamerule"
import { MSGrid, MSMapString } from "./msgrid"
import { MSLoseCause } from "./mslosecause"
import { MSRoom, MSRoomConf } from "./msroom"

/**
 * 이벤트 목록
 */
export enum MSEvents {
    /**
     * 누군가 밴 당했을 때
     */
    ban = "banned",
    /**
     * 채팅이 왔을 때
     */
    chat = "chated",
    /**
     * 설정이 바뀔 때
     */
    conf = "confed",
    /**
     * 연결 됐을 때
     */
    connect = "connected",
    /**
     * 게임 끝났을 때
     */
    end = "ended",
    /**
     * 누군가 들어왔을 때
     */
    enter = "entered",
    /**
     * 에러 *퉤*
     */
    error = "error",
    /**
     * 누군가 나갔을 때
     */
    leave = "left",
    /**
     * 돌을 옮겼을 때
     */
    move = "moved",
    /**
     * 게임이 시작됐을 때
     */
    start = "started",
    /**
     * 시간 업데이트 시
     */
    tick = "ticked",
}
export interface BanInfo extends EuserInfo<MSEvents.ban> {}
export interface ChatInfo extends EuserInfo<MSEvents.chat> {
    content:string,
}
export interface ConfInfo extends TypedInfo<MSEvents.conf> {
    conf:MSRoomConf,
}
export interface ConnectInfo extends TypedInfo<MSEvents.connect> {
    room:MSRoom,
}
export interface EndInfo extends TypedInfo<MSEvents.end> {
    loser:number;
    /**
     * 진 사람의 색깔
     */
    player:"black" | "white";
    cause:MSLoseCause;
}
export interface EnterInfo extends EuserInfo<MSEvents.enter> {
    room:MSRoom,
}
export interface ErrorInfo extends TypedInfo<MSEvents.error> {
    msg:string,
}
export interface LeaveInfo extends EuserInfo<MSEvents.leave> {}
export interface MoveInfo extends TypedInfo<MSEvents.move> {
    color:"black" | "white",
    start:MSCords,
    end:MSCords,
    dir:MSCords,
}
export interface StartInfo extends TypedInfo<MSEvents.start> {
    black:number,
    white:number,
    map:MSMapString,
    rule:MSGameRule,
    turn:"black" | "white",
}
export interface TickInfo extends TypedInfo<MSEvents.tick> {
    /**
     * 검정 경과 시간
     */
    black_time:number,
    /**
     * 하양 경과 시간
     */
    white_time:number,
    /**
     * 현재 턴의 플레이어의 제한시간
     */
    current_time:number,
}


// tslint:disable-next-line
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
    ban:{
        user:number,
    };
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


interface TypedInfo<T extends MSEvents> {
    type:T,
}
interface EuserInfo<T extends MSEvents> extends TypedInfo<T> {
    user:number,
}
export type MdEventTypes = TypedInfo<MSEvents>