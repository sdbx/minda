import { Socket } from "net"
import { SignalDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import { Immute } from "../types/deepreadonly"
import { ChatInfo, ConfInfo, ConnectInfo, EnterInfo, LeaveInfo, MdEvents, MdEventTypes, StartInfo } from "./mdevents"
import { MSGrid } from "./structure/msgrid"
import { MSRoom, MSRoomConf, MSRoomServer } from "./structure/msroom"

export class MindaRoom {
    /* public events */
    public onClose = new SignalDispatcher()
    /**
     * 방에 접속을 성공했을때 발생합니다.
     */
    public onConnect = new SimpleEventDispatcher<MSRoom>()
    /**
     * 방에 누군가 들어왔을 때 발생합니다.
     */
    public onEnter = new SimpleEventDispatcher<EnterInfo>()
    /**
     * 방에서 누군가가 채팅을 했을 때 발생합니다.
     */
    public onChat = new SimpleEventDispatcher<ChatInfo>()
    /**
     * 방설정 값이 변경되었을 때 발생합니다.
     * `conf`는 새로운 방설정 값을 의미합니다.
     */
    public onConf = new SimpleEventDispatcher<ConfInfo>()
    /**
     * 게임이 시작됐거나 이미 시작된 상태에서
     * 방에 들어왔을때 발생합니다.
     */
    public onStart = new SimpleEventDispatcher<StartInfo>()
    /**
     * 유저가 게임서버를 뿅 나왔을때
     * 발생합니다.
     */
    public onLeave = new SimpleEventDispatcher<LeaveInfo>()
    /* Access Helper */
    /**
     * 방 ID
     */
    public get id() {
        return this.info.id
    }
    /**
     * 유저 목록
     */
    public get users() {
        if (this.info.users == null) {
            return []
        }
        return [...this.info.users]
    }
    /**
     * 방 설정
     */
    public get config() {
        return {
            ...this.info.conf
        } as MSRoomConf
    }
    /**
     * 검은 돌 유저ID
     */
    public get black() {
        return this.info.conf.black
    }
    /**
     * 하얀 돌 유저ID
     */
    public get white() {
        return this.info.conf.white
    }
    /* values */
    public turn:"black" | "white"
    public board:MSGrid
    /* etc */
    public connected = false
    /* protectes */
    // protected events
    protected onSocketOpen = new SignalDispatcher()
    protected onSocketDrain = new SignalDispatcher()
    protected onSocketError = new SimpleEventDispatcher<Error>()
    protected onSocketData = new SimpleEventDispatcher<ArrayBuffer>()
    /**
     * 소-켓
     * 
     * https://www.npmjs.com/package/emailjs-tcp-socket
     */
    protected socket:Socket
    protected cache:string
    /**
     * 방 정보 (interface)
     */
    protected info:Immute<MSRoom>
    public constructor(serverInfo:MSRoomServer) {
        const [ip, port] = serverInfo.addr.split(":")
        this.socket = new Socket()
        this.socket.on("connect", () => this.onSocketOpen.dispatch())
        this.socket.on("drain", () => this.onSocketDrain.dispatch())
        this.socket.on("error", (error:Error) => this.onSocketError.dispatch(error))
        this.socket.on("data", (arraybuffer:ArrayBuffer) => this.onSocketData.dispatch(arraybuffer))
        this.socket.on("close", () => this.onClose.dispatch())
        // debug
        this.onSocketData.sub(this.onData.bind(this))
        this.socket.connect(Number.parseInt(port), ip, () => {
            this.connected = true
            this.cache = ""
            this.send("connect", {
                invite: serverInfo.invite, 
            })
        })
    }
    /**
     * 기초적인 방 정보 업데이트
     * @param roomInfo 방 정보
     */
    public updateInfo(roomInfo:Immute<MSRoom>) {
        this.info = roomInfo
    }
    /**
     * 채팅을 보냅니다.
     * @param chat 글자
     */
    public sendChat(chat:string) {
        if (!this.connected) {
            return
        }
        this.send("chat", {
            content: chat,
        })
    }

    public close() {
        this.socket.end()
    }
    protected async onData(buf:ArrayBuffer) {
        const raw = Buffer.from(buf)
        const data = raw.toString("utf8")
        this.cache += data
        while (this.cache.indexOf("\n") >= 0) {
            const msg = this.cache.substring(0, this.cache.indexOf("\n"))
            this.cache = this.cache.substring(this.cache.indexOf("\n") + 1)
            // process msg now.
            const event:MdEventTypes = JSON.parse(msg)
            const type = event.type
            switch (type) {
                case MdEvents.enter: {
                    const enter = event as EnterInfo
                    const users = this.users
                    if (users.find((v) => v === enter.user) == null) {
                        users.push(enter.user)
                    }
                    this.info = {
                        ...this.info,
                        users,
                    }
                    this.onEnter.dispatch(enter)
                } break
                case MdEvents.connect: {
                    const connect = event as ConnectInfo
                    this.info = {
                        ...this.info,
                        ...connect.room,
                    }
                    this.onConnect.dispatch(connect.room)
                } break
                case MdEvents.chat: {
                    const chatE = event as ChatInfo
                    this.onChat.dispatch(chatE)
                } break
                case MdEvents.conf: {
                    const confE = event as ConfInfo
                    this.info = {
                        ...this.info,
                        conf: confE.conf,
                    }
                    this.onConf.dispatch(confE)
                } break
                case MdEvents.start: {
                    const startE = event as StartInfo
                    this.info = {
                        ...this.info,
                        conf: {
                            ...this.info.conf,
                            black: startE.black,
                            white: startE.white,
                        }
                    }
                    this.turn = startE.turn
                    this.board = startE.board
                    this.onStart.dispatch(startE)
                } break
                case MdEvents.leave: {
                    const leaveE = event as LeaveInfo
                    const users = this.users
                    const index = users.findIndex((v) => v === leaveE.user)
                    if (index >= 0) {
                        users.splice(index, 1)
                        this.info = {
                            ...this.info,
                            users,
                        }
                    }
                    this.onLeave.dispatch(leaveE)
                } break
                default: {
                    console.log(type + " / " + JSON.stringify(event, null, 2))
                    break
                }
            }
        }
    }
    protected async send(type:string, param:{[key in string]:string} = {}) {
        if (!this.connected) {
            return
        }
        const json = JSON.stringify({
            "type": type,
            ...param,
        })
        this.socket.write(json + "\n")
    }
}