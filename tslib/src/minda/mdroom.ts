import { Socket } from "net"
import { EventDispatcher, SignalDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import { Immute } from "../types/deepreadonly"
import { Serializable, SerializeObject } from "../types/serializable"
import { ChatInfo, ConfInfo, ConnectInfo, EnterInfo,
    ErrorInfo, LeaveInfo, MdCommands, MdEvents,
    MdEventTypes, StartInfo } from "./mdevents"
import { MSGrid } from "./structure/msgrid"
import { MSRoom, MSRoomConf, MSRoomServer } from "./structure/msroom"

/**
 * 민다룸 백앤드 (방목록 동기화)
 */
class MindaRoomBase implements MSRoom {
    /* MSRoom implements */
    public id:string
    public created_at:number
    public conf:MSRoomConf
    public users:number[] = []
    public ingame:boolean
    /* MSRoom access helper */
    /**
     * 검은 돌의 유저ID
     */
    public get black() {
        return this.conf.black
    }
    /**
     * 하얀 돌의 유저ID
     */
    public get white() {
        return this.conf.white
    }
    /* Value implements */
    /**
     * 채팅의 메시지들 입니다.
     */
    public messages:string[] = []
    /**
     * @todo 이차원 배열이라고 하는데 난 몰라
     */
    public board:string[][] = []
    /**
     * 누구의 턴인가
     */
    public turn:"black" | "white"
    /* Public events implements */
    /**
     * 방에 접속을 성공했을때 발생합니다.
     */
    public onConnect = new SimpleEventDispatcher<MSRoom>()
    /**
     * 방에서 누군가가 채팅을 했을 때 발생합니다.
     */
    public onChat = new MindaEventDispatcher<ChatInfo>()
    /**
     * 방설정 값이 변경되었을 때 발생합니다.
     * `conf`는 새로운 방설정 값을 의미합니다.
     */
    public onConf = new SimpleEventDispatcher<MSRoomConf>()
    /**
     * 게임이 시작됐거나 이미 시작된 상태에서
     * 방에 들어왔을때 발생합니다.
     */
    public onStart = new MindaEventDispatcher<StartInfo>()
    /**
     * 방에 누군가 들어왔을 때 발생합니다.
     */
    public onEnter = new MindaEventDispatcher<EnterInfo>()
    /**
     * 유저가 게임서버를 뿅 나왔을때
     * 발생합니다.
     */
    public onLeave = new MindaEventDispatcher<LeaveInfo>()
    /**
     * 보낸 명령에 문제가 있을 시에 발생합니다.
     */
    public onMindaError = new SimpleEventDispatcher<string>()
    /* Unique implements */
    /**
     * 현재 소켓서버랑 연결됐는지 여부
     */
    public connected = false
    /* protected events */
    protected onEvents = new EventDispatcher<MdEvents, unknown>()
    protected onSocketOpen = new SignalDispatcher()
    protected onSocketDrain = new SignalDispatcher()
    protected onSocketError = new SimpleEventDispatcher<Error>()
    protected onSocketData = new SimpleEventDispatcher<ArrayBuffer>()
    protected onSocketClose = new SignalDispatcher()
    /**
     * [소-켓](https://www.npmjs.com/package/emailjs-tcp-socket)
     */
    protected socket:Socket
    /**
     * 웹소켓 캐시
     */
    protected cache:string
    public constructor(serverInfo:MSRoomServer) {
        const [ip, port] = serverInfo.addr.split(":")
        this.socket = new Socket()
        this.socket.on("connect", () => this.onSocketOpen.dispatch())
        this.socket.on("drain", () => this.onSocketDrain.dispatch())
        this.socket.on("error", (error:Error) => this.onSocketError.dispatch(error))
        this.socket.on("data", (arraybuffer:ArrayBuffer) => this.onSocketData.dispatch(arraybuffer))
        this.socket.on("close", () => this.onSocketClose.dispatch())
        // debug
        this.onSocketData.sub(this.onRawPacket.bind(this))
        this.socket.connect(Number.parseInt(port), ip, () => {
            this.connected = true
            this.cache = ""
            this.send("connect", {
                invite: serverInfo.invite,
            })
        })
    }
    public close() {

    }
    protected async handleRoomPacket(type:MdEvents, event:unknown) {
        switch (type) {
            case MdEvents.chat: {
                const chatE = event as ChatInfo
                this.messages.push(chatE.content)
                this.onChat.dispatch(chatE)
            } break
            case MdEvents.conf: {
                const confE = event as ConfInfo
                this.conf = confE.conf
                this.onConf.dispatch(confE.conf)
            } break
            case MdEvents.connect: {
                const connect = event as ConnectInfo
                const room = connect.room
                this.id = room.id
                this.created_at = room.created_at
                this.conf = room.conf
                this.users = room.users
                this.ingame = room.ingame
                this.onConnect.dispatch({
                    ...connect.room
                })
            } break
            case MdEvents.enter: {
                const enter = event as EnterInfo
                const users = [...this.users]
                if (users.find((v) => v === enter.user) == null) {
                    users.push(enter.user)
                    this.users = users
                }
                this.onEnter.dispatch(enter)
            } break
            case MdEvents.error: {
                const errorE = event as ErrorInfo
                this.onMindaError.dispatch(errorE.msg)
                console.error(`MindaError: ${errorE.msg}`)
            } break
            case MdEvents.leave: {
                const leaveE = event as LeaveInfo
                const users = [...this.users]
                const index = users.findIndex((v) => v === leaveE.user)
                if (index >= 0) {
                    users.splice(index, 1)
                    this.users = users
                }
                this.onLeave.dispatch(leaveE)
            } break
            case MdEvents.move: {
                // @TODO 몬가..
            } break
            case MdEvents.start: {
                const startE = event as StartInfo
                this.conf.black = startE.black
                this.conf.white = startE.white
                this.turn = startE.turn
                this.board = startE.board
                this.onStart.dispatch(startE)
            } break
            default: {
                console.log(type + " / " + JSON.stringify(event, null, 2))
                break
            }
        }
    }
    /**
     * 소켓에게 명령을 보냅니다
     * @param type 명령 타입
     * @param param 명령 파라메터
     */
    protected async send<T extends keyof MdCommands>(type:T, param?:MdCommands[T]) {
        if (param == null) {
            param = {}
        }
        return this.sendRaw(type, param)
    }
    /**
     * 소켓에게 깡 type을 보냅니다.
     * @param type 명령 타입
     * @param param 명령 파라메터
     */
    protected async sendRaw(type:string, param:object = {}) {
        if (!this.connected) {
            return
        }
        const json = JSON.stringify({
            "type": type,
            ...param,
        })
        this.socket.write(json + "\n")
    }
    /**
     * 소켓에 데이터가 왔을 때 핸들링합니다.
     * Raw패킷만 핸들링합니다.
     * @param buf 버퍼[]
     */
    private async onRawPacket(buf:ArrayBuffer) {
        const raw = Buffer.from(buf)
        const data = raw.toString("utf8")
        this.cache += data
        while (this.cache.indexOf("\n") >= 0) {
            const msg = this.cache.substring(0, this.cache.indexOf("\n"))
            this.cache = this.cache.substring(this.cache.indexOf("\n") + 1)
            // process msg now.
            const event:MdEventTypes = JSON.parse(msg)
            const type = event.type
            this.onEvents.dispatch(type, event)
        }
    }
}
export class MindaRoom extends MindaRoomBase {
    public constructor(serverInfo:MSRoomServer) {
        super(serverInfo)
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
}
class MindaEventDispatcher<T> extends SimpleEventDispatcher<Exclude<T, "type">> {}