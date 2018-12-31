import { Socket } from "net"
import { SignalDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import { Immute } from "../types/deepreadonly"
import { ChatInfo, ConnectInfo, EnterInfo, MdEvents, MdEventTypes } from "./mdevents"
import { MSRoom, MSRoomServer } from "./structure/msroom"

export class MindaRoom {
    /* public events */
    public onConnect = new SignalDispatcher()
    public onChat = new SimpleEventDispatcher<ChatInfo>()
    /* Access Helper */
    public get id() {
        return this.info.id
    }
    public get users() {
        if (this.info.users == null) {
            return []
        }
        return [...this.info.users]
    }
    /* etc */
    public connected = false
    /* protectes */
    // protected events
    protected onSocketOpen = new SignalDispatcher()
    protected onSocketDrain = new SignalDispatcher()
    protected onSocketError = new SimpleEventDispatcher<Error>()
    protected onSocketData = new SimpleEventDispatcher<ArrayBuffer>()
    protected onSocketClose = new SignalDispatcher()
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
        this.socket.on("close", () => this.onSocketClose.dispatch())
        // debug
        this.onSocketData.sub(this.onData.bind(this))
        this.socket.connect(Number.parseInt(port), ip, () => {
            console.log("Connected")
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
                    const user = event as EnterInfo
                    const users = this.users
                    if (users.find((v) => v === user.user) == null) {
                        users.push(user.user)
                    }
                    this.info = {
                        ...this.info,
                        users,
                    }
                } break
                // connected
                case MdEvents.connect: {
                    const room = event as ConnectInfo
                    this.info = {
                        ...this.info,
                        ...room.room,
                    }
                    this.onConnect.dispatch()
                } break
                case MdEvents.chat: {
                    const chatE = event as ChatInfo
                    this.onChat.dispatch(chatE)
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
        this.socket.write(json)
    }
}