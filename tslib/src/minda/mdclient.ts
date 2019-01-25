import Diff from "deep-diff"
import fetch from "node-fetch"
import { SignalDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import { Immute } from "../types/deepreadonly"
import { TimerID, WebpackTimer } from "../webpacktimer"
import { defaultProfile, mdtimeout } from "./mdconst"
import { MindaCredit } from "./mdcredit"
import { extractContent, reqBinary, reqGet, reqPost } from "./mdrequest"
import { MindaRoom } from "./mdroom"
import { MSGameRule } from "./structure/msgamerule"
import { MSRoom, MSRoomConf, MSRoomServer } from "./structure/msroom"
import { MSUser } from "./structure/msuser"
import awaitEvent from "./util/timeout"
/**
 * 민다 로비 클라이언트
 */
export class MindaClient {
    /**
     * 방 목록 (immutable)
     */
    public rooms:Immute<MSRoom[]> = []
    public me:MSUser
    /**
     * 방목록 동기화 및 프로필을 불러왔을때
     */
    public readonly onReady = new SignalDispatcher()
    /**
     * 방이 추가됐을때
     */
    public readonly onRoomAdded = new SimpleEventDispatcher<MSRoom[]>()
    /**
     * 방이 없어졌을때
     */
    public readonly onRoomRemoved = new SimpleEventDispatcher<MSRoom[]>()
    /**
     * 방이 몬가.. 변경되었을때
     */
    public readonly onRoomUpdated = new SimpleEventDispatcher<MSRoom[]>()
    /**
     * 들어간 방 목록
     */
    protected connectedRooms:Map<string, MindaRoom> = new Map()
    /**
     * 인증 토큰
     */
    protected token:string
    /**
     * 기본 프로필 이미지
     */
    protected defaultPicture:Buffer
    protected syncer:TimerID
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
        // this.init().then(() => this.onReady.dispatch())
    }
    /**
     * 기초적인 비동기적 설정을 합니다.
     */
    public async init() {
        this.defaultPicture = await fetch(defaultProfile).then((v) => v.buffer())
        await this.getMyself()
        await this.fetchRooms()
        await this.sync()
        this.autoSync()
    }
    /**
     * 방 목록을 스스로 동기화하게 만듭니다.
     * @param enable 활성화 / 비활성화
     */
    public autoSync(enable = true) {
        if (this.syncer != null) {
            WebpackTimer.clearInterval(this.syncer)
        }
        if (enable) {
            this.syncer = WebpackTimer.setInterval(() => {
                this.sync()
            }, 10000)
        }
    }
    /**
     * 방 목록을 불러옵니다.
     */
    public async fetchRooms() {
        const rooms = await extractContent<MSRoom[]>(reqGet("GET", "/rooms/", this.token))
        rooms.sort((a,b) => a.created_at - b.created_at)
        if (this.rooms != null) {
            const added:MSRoom[] = []
            const deleted:MSRoom[] = []
            const updated:MSRoom[] = []
            const orgRoom = [...this.rooms] as MSRoom[]
            for (const room of rooms) {
                const orgI = orgRoom.findIndex((v) => v.id === room.id)
                if (orgI >= 0) {
                    const diff = Diff.diff(orgRoom[orgI], room)
                    if (diff != null && diff.length >= 1) {
                        updated.push(room)
                    }
                    orgRoom.splice(orgI, 1)
                } else {
                    added.push(room)
                }
            }
            deleted.push(...orgRoom)
            if (added.length >= 1) {
                this.onRoomAdded.dispatch(added)
            }
            if (updated.length >= 1) {
                this.onRoomUpdated.dispatch(updated)
            }
            if (deleted.length >= 1) {
                this.onRoomRemoved.dispatch(deleted)
            }
        }
        this.rooms = [...rooms]
        return this.rooms
    }
    /**
     * 방을 만듭니다.
     * @param roomConf 방설정
     * @returns 방 혹은 null (실패)
     */
    public async createRoom(name:string, open = true) {
        const roomServer = await extractContent<MSRoomServer>(
            reqPost("POST", `/rooms/`, this.token, {
                name,
                king: this.me.id,
                black: -1,
                white: -1,
                open,
            }))
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
    /**
     * 유저의 프로필 이미지를 불러옵니다.
     * @param id 유저의 이미지 ID 혹은 유저
     */
    public async getUserImage(id:number | MSUser) {
        if (typeof id === "object") {
            id = id.picture
        }
        if (id == null || id < 0) {
            return this.defaultPicture
        }
        const buf = await reqBinary("GET", `/pics/${id}/`, this.token)
        if (buf.ok) {
            return buf.buffer()
        } else {
            return this.defaultPicture
        }
    }
    /**
     * 자신의 프로필 이미지를 설정합니다.
     * @param image 이미지(Buffer)
     */
    public async setProfileImage(image:Buffer) {
        const result = await reqBinary("POST", "/pics/", this.token, image)
        if (result.ok) {
            await this.getMyself()
            return extractContent<{pic_id:number}>(result)
                .then((v) => v.pic_id)
        } else {
            return -1
        }
    }
    /**
     * [내부] 자신 스스로의 프로필을 가져옵니다.
     */
    public async getMyself() {
        const myself = await extractContent<MSUser>(reqGet("GET", "/users/me/", this.token))
        this.me = myself
    }
    /**
     * [내부] 동기화합니다.
     */
    protected async sync() {
        await this.fetchRooms()
    }
    /**
     * [내부] 방에 연결합니다
     * @param roomServer 방 서버 정보 
     */
    protected async connectRoom(roomServer:MSRoomServer) {
        try {
            const mindaRoom = new MindaRoom(roomServer, this.me)
            return awaitEvent(mindaRoom.onConnect, mdtimeout, () => {
                this.connectedRooms.set(mindaRoom.id, mindaRoom)
                return mindaRoom
            })
        } catch (err) {
            console.error(err)
            return null
        }
    }
    protected getRoomID(id:string | Immute<MSRoom>) {
        if (typeof id === "string") {
            return id
        } else {
            return id.id
        }
    }
}