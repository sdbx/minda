import Diff from "deep-diff"
import fetch from "node-fetch"
import { SignalDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import awaitEvent from "../timeout"
import { Immute } from "../types/deepreadonly"
import { TimerID, WebpackTimer } from "../webpacktimer"
import { mdtimeout } from "./mdconst"
import { MindaCredit } from "./mdcredit"
import { extractContent, reqBinaryGet, reqBinaryPost, reqGet, reqPost } from "./mdrequest"
import { MindaRoom } from "./mdroom"
import { MSGameRule } from "./structure/msgamerule"
import { MSInventory } from "./structure/msinventory"
import { MSRecStat } from "./structure/msrecstat"
import { MSRoom, MSRoomConf, MSRoomServer } from "./structure/msroom"
import { MSSkin } from "./structure/msskin"
import { MSUser } from "./structure/msuser"
/**
 * 민다 로비 클라이언트
 */
export class MindaClient {
    /**
     * 방 목록 (immutable)
     */
    public rooms:Immute<MSRoom[]> = []
    /**
     * 내 자신 정보
     */
    public me:MSUser & {skins:MSSkin[]}
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
     * 토큰으로 방 목록 및 유저 정보를 불러옵니다.
     */
    public async login() {
        this.defaultPicture = null
        try {
            await this.getMyself()
            await this.fetchRooms()
            await this.sync()
            this.autoSync()
            return true
        } catch (err) {
            console.error(err)
        }
        return false
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
        const buf = await reqBinaryGet("GET", `/pics/${id}/`, this.token)
        if (buf.ok) {
            return buf.buffer()
        } else {
            return this.defaultPicture
        }
    }
    /**
     * 유저 정보를 불러옵니다.
     * 
     * 이미지는 `getProfileImage` 사용바람.
     * @param id 유저 ID
     */
    public async user(id:number | MSUser) {
        if (typeof id === "object") {
            id = id.id
        }
        const user = await extractContent<MSUser>(reqGet("GET", `/users/${id}/`, this.token))
        return user
    }
    /**
     * 자신의 프로필 이미지를 설정합니다.
     * @param image 이미지(Buffer)
     */
    public async setProfileImage(image:string | Buffer) {
        if (typeof image === "string") {
            image = await fetch(image).then((v) => v.blob())
        }
        const result = await reqBinaryPost("POST", "/pics/", {
            file: image,
        }, this.token)
        if (result.ok) {
            await this.getMyself()
            return extractContent<{pic_id:number}>(result)
                .then((v) => v.pic_id)
        } else {
            return -1
        }
    }
    /**
     * 유저의 전적을 불러옵니다.
     * @param options
     * @param user 유저아이디입니다. 해당 유저가 포함된 전적만 가져옵니다.
     * @param since 시간입니다.  해당 시간 이후의 전적만 가져옵니다.
     * @param p 페이지 번호입니다. 1부터 시작합니다.
     */
    public async searchRecords(options:Partial<{
        user:number | MSUser,
        since:number | Date,
        p:number,
    }> = {}, limit = 100) {
        if (options.user != null && typeof options.user !== "number") {
            options.user = options.user.id
        }
        if (options.since != null && typeof options.since !== "number") {
            options.since = options.since.getTime()
        }
        const result = await extractContent<MSRecStat[]>(reqGet("GET", `/histories/`, this.token))
        return result
    }
    /**
     * [내부] 자신 스스로의 프로필을 가져옵니다.
     * 
     * 스킨도 가져오고 싱크 기능도 합니다.
     */
    public async getMyself() {
        const myself = await extractContent<MSUser>(reqGet("GET", "/users/me/", this.token))
        const skins = await extractContent<MSSkin[]>(reqGet("GET", "/skins/me/", this.token))
        this.me = {
            ...myself,
            skins,
        }
        return {...this.me}
    }
    /**
     * ID로 스킨을 가져옵니다.
     * @param id 스킨 ID
     */
    public async getSkinById(id:number) {
        const skin = await extractContent<MSSkin>(reqGet("GET", `/skins/${id}/`, this.token))
        if (skin == null) {
            return null
        }
        return {
            ...skin,
            whiteImage: await fetch(skin.white_picture).then((v) => v.blob()),
            blackImage: await fetch(skin.black_picture).then((v) => v.blob())
        }
    }
    /**
     * 유저의 스킨을 가져옵니다.
     * 
     * 없으면 null 반환
     * @param user 유-저
     */
    public async getSkinOfUser(user:number | MSUser) {
        const parseUser = await this.user(user)
        const skin = parseUser.inventory.current_skin
        if (skin != null) {
            return this.getSkinById(skin)
        } else {
            return null
        }
    }
    /**
     * 슬롯에 스킨을 배정합니다.
     * 마네킹 같은 시스템이긴 한데
     * 이게 왜 또 백인지 흑인지 모르겠음
     * @param slot 
     * @param name 
     * @param image 
     */
    public async setSkinSlotN(slot:1 | 2, name:string, black:string | Buffer, white?:string | Buffer) {
        await this.getMyself()
        const getCoin = (inv:MSInventory) => {
            switch (slot) {
                case 1:
                    return inv.one_color_skin
                case 2:
                    return inv.two_color_skin
                default:
                    return inv.current_skin
            }
        }
        let numCode:string
        switch (slot) {
            case 1:
                numCode = "one"; break
            case 2:
                numCode = "two"; break
            default:
                numCode = "zero"
        }
        const coin = getCoin(this.me.inventory)
        if (coin <= 0) {
            throw new Error("Not enough money")
        }
        const getImage = async (image:string | Buffer) => {
            if (typeof image === "string") {
                return fetch(image).then((v) => v.blob())
            } else {
                return image
            }
        }
        black = await getImage(black)
        if (white == null) {
            white = black
        } else {
            white = await getImage(white)
        }
        await reqBinaryPost("POST", `/skins/me/${numCode}/`, {
            name,
            white,
            black,
        }, this.token)
        await this.getMyself()
        if (getCoin(this.me.inventory) < coin) {
            return this.me.skins.find((v) => v.name === name)
        } else {
            return null
        }
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