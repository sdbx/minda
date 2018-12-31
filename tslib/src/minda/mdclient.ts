import { Immute } from "../types/deepreadonly"
import { TimerID, WebpackTimer } from "../webpacktimer"
import { extractContent, reqGet, reqPost } from "./mdrequest"
import { MindaRoom } from "./mdroom"
import { MSRoom, MSRoomServer } from "./structure/msroom"
/**
 * 민다 서버 방 목록
 */
export class MindaClient {
    /**
     * 방 목록 (immutable)
     */
    public rooms:Immute<MSRoom[]> = []
    /**
     * 들어간 방 목록
     */
    protected joinedRooms:Map<string, MindaRoom> = new Map()
    protected token:string
    /**
     * 새로운 민다-클라를 생성합니다.
     * @param token `MindaCredit`으로 얻은 토큰
     */
    public constructor(token:string) {
        this.token = token
    }
    /**
     * 방 목록을 불러옵니다.
     */
    public async fetchRoom() {
        const rooms = await extractContent<MSRoom[]>(reqGet("GET", "/rooms/", this.token))
        rooms.sort((a,b) => a.created_at - b.created_at)
        this.rooms = rooms
        return this.rooms
    }
    /**
     * 게임서버로 연결하기 위해 방에 접속합니다.
     * @param room 방
     * @returns 방 혹은 null (실패)
     */
    public async connectRoom(room:string | Immute<MSRoom>) {
        room = this.getRoomID(room)
        try {
            const inviteCode = await extractContent<MSRoomServer>(
                reqPost("PUT", `/rooms/${room}/`, this.token))
            const mindaRoom = new MindaRoom(this.rooms.find((v) => v.id === room), inviteCode)
            console.log(inviteCode)
            this.joinedRooms.set(room, mindaRoom)
            return new Promise<MindaRoom>((res, rej) => {
                let timer:TimerID
                const fn = mindaRoom.onConnect.one(() => {
                    WebpackTimer.clearTimeout(timer)
                    res(mindaRoom)
                })
                timer = WebpackTimer.setTimeout(() => {
                    mindaRoom.onConnect.unsub(fn)
                    rej(new Error("TIMEOUT"))
                }, 5000)
            })
        } catch (err) {
            console.error(err)
            return null
        }
    }
    private getRoomID(id:string | Immute<MSRoom>) {
        if (typeof id === "string") {
            return id
        } else {
            return id.id
        }
    }
}