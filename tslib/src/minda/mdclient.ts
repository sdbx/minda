import Diff from "deep-diff"
import fs from "fs-extra"
import fetch from "node-fetch"
import path from "path"
import { SignalDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import awaitEvent from "../timeout"
import { Immute } from "../types/deepreadonly"
import { TimerID, WebpackTimer } from "../webpacktimer"
import { mdtimeout } from "./mdconst"
import { MindaCredit } from "./mdcredit"
import { MindaError } from "./mderror"
import { extractContent, reqBinaryGet, reqBinaryPost, reqGet, reqPost } from "./mdrequest"
import { MindaRoom } from "./mdroom"
import { MSGameRule } from "./structure/msgamerule"
import { MSInventory } from "./structure/msinventory"
import { MSRecStat } from "./structure/msrecstat"
import { MSRoom, MSRoomConf, MSRoomServer } from "./structure/msroom"
import { MSSkin, SkinBinary } from "./structure/msskin"
import { MSUPicture, MSUser } from "./structure/msuser"
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
     * 유저 정보를 불러옵니다.
     * 
     * 자세한 정보는 `getProfile` 함수를 사용해주세요.
     * @param id 유저 ID
     */
    public async user(id?:number | MSUser) {
        if (id == null) {
            return this.getMyself()
        }
        if (typeof id === "object") {
            id = id.id
        }
        const user = await extractContent<MSUser>(reqGet("GET", `/users/${id}/`, this.token))
        return user
    }
    /**
     * 유저의 프로필을 불러옵니다. (스킨, 프로필 포함.)
     * 
     * 유저가 없으면 *null*을 반환합니다.
     * 
     * 스킨이 없으면 `skin`이 *null*을 반환합니다.
     * 
     * 프로필 이미지가 없으면 `picture_image`가 *null*을 반환합니다.
     * @param id 유저 ID
     */
    public async getProfile(id?:number | MSUser) {
        let u:MSUser
        try {
            u = await this.user(id)
        } catch (err) {
            console.log(err)
            return null
        }
        let profileI:Buffer
        if (u.picture == null) {
            profileI = null
        } else {
            profileI = await fetch(u.picture).then((v) => v.buffer())
        }
        return {
            ...u,
            picture_image: profileI,
            skin: await this.getSkinOfUser(u),
        } as MSUser & MSUPicture & {skin:MSSkin & SkinBinary}
    }
    /**
     * 자신의 프로필 이미지를 설정합니다.
     * @param image 이미지(Buffer)
     */
    public async setProfile(nickname:string, image?:string) {
        const me = await this.getMyself()
        if (image != null) {
            const url = await reqBinaryPost("PUT", "/users/me/picture/", {
                file: await this.getImage(image),
            }, this.token)
            if (!url.ok) {
                throw new MindaError(url)
            }
        }
        if (nickname != null) {
            const url = await reqPost("PUT", "/users/me/", this.token, {
                username: nickname == null ? me.username : nickname,
            })
            if (!url.ok) {
                throw new MindaError(url)
            }
        }
        return this.getMyself()
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
        const toStr = (n:number | MSUser | Date) => n == null ? n as null : n.toString()
        const result = await extractContent<MSRecStat[]>(reqGet("GET", `/histories/`, this.token, {
            user: toStr(options.user),
            since: toStr(options.since),
            p: toStr(options.p),
        }))
        return result
    }
    /**
     * [내부] 자신 스스로의 프로필을 가져옵니다.
     * 
     * 스킨도 가져오고 싱크 기능도 합니다.
     */
    public async getMyself() {
        const myself = await extractContent<MSUser>(reqGet("GET", "/users/me/", this.token))
        this.me = myself
        return {...this.me}
    }
    /**
     * 자신이 소유한 스킨들을 가져옵니다.
     * @returns [바이너리] 스킨 데이터
     */
    public async getMySkins() {
        const skins = await extractContent<MSSkin[]>(reqGet("GET", "/skins/me/", this.token))
        const skinBinary:Array<MSSkin & SkinBinary> = []
        for (const skin of skins) {
            skinBinary.push(await this.getSkinById(skin.id))
        }
        return skinBinary
    }
    /**
     * ID로 스킨을 가져옵니다.
     * @param id 스킨 ID
     * @returns [바이너리] 스킨 데이터
     */
    public async getSkinById(id:number) {
        const skin = await extractContent<MSSkin>(reqGet("GET", `/skins/${id}/`, this.token))
        if (skin == null) {
            return null
        }
        return {
            ...skin,
            whiteImage: await fetch(skin.white_picture).then((v) => v.buffer()),
            blackImage: await fetch(skin.black_picture).then((v) => v.buffer())
        } as MSSkin & SkinBinary
    }
    /**
     * 유저의 스킨을 가져옵니다.
     * 
     * 없으면 *null* 반환
     * @param user 유-저
     * @returns [바이너리] 스킨 데이터
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
     * 가지고 있는 스킨을 낍니다.
     * @param skin 스킨
     */
    public async setSkin(skin:MSSkin | number) {
        try {
            return reqPost("PUT", "/skins/me/current/", this.token, {
                id: typeof skin === "number" ? skin : skin.id
            }).then((v) => v.ok ? this.getProfile() : null)
        } catch (err) {
            console.error(err)
            return null
        }
    }
    /**
     * 스킨을 추가합니다.
     * 장착은 따로 해주세요.
     * @param name 스킨 이름
     * @param black 검정색 이미지 혹은 파일
     * @param white 하얀색 이미지 혹은 파일 (없을 시 검은색과 동일)
     */
    public async addSkin(name:string, black:string, white?:string) {
        await this.getMyself()
        const slot = white == null ? 1 : 2
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
        let param:object
        if (slot === 2) {
            param = {
                black: await this.getImage(black),
                white: await this.getImage(white),
            }
        } else {
            param = {
                file: await this.getImage(black)
            }
        }
        const res = await reqBinaryPost("POST", `/skins/me/${numCode}/`, {
            name,
            ...param,
        }, this.token)
        if (res.ok) {
            return (await this.getMySkins()).find((v) => v.name === name)
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
     * [내부] 외부 & 내부 경로로부터 이미지 Buffer를 불러옵니다.
     * @param imagePath 이미지 경로
     */
    protected async getImage(imagePath:string) {
        if (imagePath.startsWith("http")) {
            const fname = imagePath.match(/[=\/].+\.(png|jpg|gif)/i)
            return {
                filename: fname != null ? fname[0].substr(1) : "unknown.png",
                buf: await fetch(imagePath).then((v) => v.buffer()),
            }
        } else {
            const fname = imagePath.substr(imagePath.lastIndexOf(path.sep) + 1)
            return {
                filename: fname,
                buf: await fs.readFile(fname),
            }
        }
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