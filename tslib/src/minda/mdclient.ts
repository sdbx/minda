import { Immute } from "../types/deepreadonly"
import { TimerID, WebpackTimer } from "../webpacktimer"
import { MindaCredit } from "./mdcredit"
import { extractContent, reqGet, reqPost } from "./mdrequest"
import { MindaRoom } from "./mdroom"
import { MSRoom, MSRoomConf, MSRoomServer } from "./structure/msroom"
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
    protected connectedRooms:Map<string, MindaRoom> = new Map()
    protected token:string
    /**
     * 새로운 민다-클라를 생성합니다.
     * @param token `MindaCredit`으로 얻은 토큰
     */
    public constructor(token:string | MindaCredit) {
        if (typeof token === "string") {
            this.token = token
        } else {
            this.token = token.token
        }
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
     * 방을 만듭니다.
     * @param roomConf 방설정
     * @returns 방 혹은 null (실패)
     */
    public async createRoom(roomConf:MSRoomConf) {
        const roomServer = await extractContent<MSRoomServer>(
            reqPost("POST", `/rooms/`, this.token, roomConf))
        return this.connectRoom(roomServer)
    }
    /**
     * 방에 들어갑니다.
     * @param room 방
     * @returns 방 혹은 null (실패)
     */
    public async joinRoom(room:string | Immute<MSRoom>) {
        room = this.getRoomID(room)
        const roomServer = await extractContent<MSRoomServer>(
            reqPost("PUT", `/rooms/${room}/`, this.token))
        return this.connectRoom(roomServer)
    }
    private async connectRoom(roomServer:MSRoomServer) {
        try {
            const mindaRoom = new MindaRoom(roomServer)
            return new Promise<MindaRoom>((res, rej) => {
                let timer:TimerID
                const fn = mindaRoom.onConnect.one(() => {
                    WebpackTimer.clearTimeout(timer)
                    this.connectedRooms.set(mindaRoom.id, mindaRoom)
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