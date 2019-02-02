import { MindaAdmin, MindaClient, MindaCredit, MindaRoom, MSGrid, MSUser, StoneType } from "minda-ts"
import { MSRoom } from "minda-ts/build/main/minda/structure/msroom"
import path from "path"
import { Column, Entity, PrimaryColumn } from "typeorm"
import SnowCommand, { SnowContext } from "../snow/bot/snowcommand"
import BaseGuildCfg from "../snow/config/baseguildcfg"
import SimpleConfig from "../snow/config/simpleconfig"
import SnowConfig from "../snow/config/snowconfig"
import SnowUser from "../snow/snowuser"
import awaitEvent from "../timeout"
import { DeepReadonly } from "../types/deepreadonly"
import { bindFn } from "../util"
import { WebpackTimer } from "../webpacktimer"
import { renderBoard } from "./boardrender"
import { blank1_2, blank1_3, blank1_4, blank1_6, blankChar, blankChar2 } from "./cbconst"
import BotConfig from "./guildcfg"

export default class MindaExec {
    public commands:Array<SnowCommand<BotConfig>> = []
    protected admin:MindaAdmin
    protected dbpath:string
    protected userDB:SimpleConfig<MindaID, UserIdentifier>
    protected authQueue:Map<string, boolean> = new Map()
    protected playingQueue:Map<string, MindaRoom> = new Map()
    public constructor(adminToken:string, dir:string) {
        this.userDB = new SimpleConfig(MindaID, path.resolve(dir, "mindaid.sqlite"))
        this.admin = new MindaAdmin(adminToken)
        this.commands.push(new SnowCommand({
            name: "auth",
            paramNames: ["oAuth-공급자"],
            description: "민다 인-증을 해봅시다.",
            func: bindFn(this, this.cmdAuth),
            reqLength: 0,
        }, "string"))
        this.commands.push(new SnowCommand({
            name: "unauth",
            paramNames: [],
            description: "민다 인증을 뚜따합니다.",
            func: bindFn(this, this.cmdUnAuth),
        }))
        this.commands.push(new SnowCommand({
            name: "fight",
            paramNames: ["맞짱뜰 유저"],
            description: "싸우자",
            func: bindFn(this, this.cmdFight),
        }, "SnowUser"))
        this.commands.push(new SnowCommand({
            name: "stat",
            paramNames: ["검색할 유저"],
            description: "전적을 검색합니다.",
            func: bindFn(this, this.cmdRecordStat),
            reqLength: 0,
        }, "SnowUser"))
    }
    public async init() {
        await this.userDB.connect()
        await this.admin.login()
    }
    protected async cmdFight(context:SnowContext<BotConfig>, otherUser:SnowUser) {
        const { channel, message } = context
        const mindaUsers = await this.admin.listUsers()
        const getMindaUser = async (user:SnowUser) => {
            const u = await this.userDB.get({
                uid: user.id,
                platform: user.platform,
            }, "mindaId")
            if (u < 0) {
                return null
            }
            const minda = mindaUsers.find((v) => v.id === u)
            if (minda == null) {
                return null
            } else {
                return minda
            }
        }
        const user1 = message.author
        const user2 = otherUser
        const minda1 = await getMindaUser(user1)
        const minda2 = this.admin.me // await getMindaUser(user2)
        if (this.playingQueue.has(user1.getUID()) || this.playingQueue.has(user2.getUID())) {
            return "이미 플레이 중입니다."
        }
        if (minda1 == null || minda2 == null) {
            const noAuth:string[] = []
            if (minda1 == null) {
                noAuth.push(user1.nickname)
            }
            if (minda2 == null) {
                noAuth.push(user2.nickname)
            }
            return `${noAuth.join(", ")} 유저가 민다에 없습니다.`
        }
        const room = await this.admin.createRoom(`[${channel.name()}] ${user1.nickname} vs ${user2.nickname}`)
        let roomFind:DeepReadonly<MSRoom>
        for (let tries = 0; tries < 5; tries += 1) {
            roomFind = (await this.admin.fetchRooms()).find((v) => v.id === room.id)
            if (roomFind != null) {
                break
            }
            if (tries >= 4) {
                return `방 생성에 실패했습니다.`
            }
            await new Promise<void>((res, rej) => {
                WebpackTimer.setTimeout(() => res(),300)
            })
        }
        await channel.send("방 이름: " + roomFind.conf.name)
        /**
         * Debug
         */
        await room.setWhite(minda2)

        this.playingQueue.set(user1.getUID(), room)
        this.playingQueue.set(user2.getUID(), room)
        room.onChat.sub((ch) => {
            const n = this.admin.users.find((v) => v.id === ch.user).username
            channel.send(`${n} : ${ch.content}`)
        })
        room.onLeave.sub(async (lf) => {
            if (room.ingame) {
                if (lf.user === minda1.id) {
                    await room.sendChat(`선수 ${minda1.username}이(가) 나갔습니다.`)
                } else if (lf.user === minda2.id) {
                    await room.sendChat(`선수 ${minda2.username}이(가) 나갔습니다.`)
                }
            }
        })
        room.onEnd.sub(async (event) => {
            let winner:string
            let color:"검은 돌" | "하얀 돌"
            if (event.loser === room.black) {
                winner = (await this.admin.user(room.white)).username
                color = "하얀 돌"
            } else {
                winner = (await this.admin.user(room.black)).username
                color = "검은 돌"
            }
            await channel.send(`${winner} (${color}) 승리!`)
            room.close()
        })
        room.onStart.sub(async (si) => {
            const {blackStone, whiteStone} = context.configGroup
            await channel.send(null, await renderBoard(room.board))
            // await channel.send(`게임 시작.\n${}`)
        })
        awaitEvent(room.onEnter, 60000, async (info) => {
            if (info.user === minda1.id) {
                if (await room.setBlack(minda1)) {
                    await room.sendChat(`흑돌 선수 ${minda1.username}님이 입장합니다.`)   
                } else {
                    channel.send("흑돌을 설정하는데 실패했습니다.")
                }
            } else if (info.user === minda2.id) {
                if (await room.setWhite(minda2)) {
                    await room.sendChat(`백돌 선수 ${minda2.username}님이 입장합니다.`)
                } else {
                    channel.send("백돌을 설정하는데 실패했습니다.")
                }
            }
            if (room.black >= 0 && room.white >= 0) {
                if (await room.startGame()) {
                    await room.sendChat("경기를 시작합니다.")
                    return true
                }
            }
            return null
        }, true).catch(async () => {
            room.close()
            this.playingQueue.delete(user1.getUID())
            this.playingQueue.delete(user2.getUID())
            await channel.send("유저가 접속을 안하여 방이 닫혔습니다.")
        })
        return null
    }
    protected async cmdUnAuth(context:SnowContext<BotConfig>) {
        const { channel, message } = context
        const user = message.author
        const uid = {
            uid: user.id,
            platform: user.platform,
        }
        const id = await this.userDB.get(uid, "mindaId")
        if (id < 0) {
            return "인증된 계정이 없습니다."
        }
        await this.userDB.set(uid, "mindaId", -1)
        return `${channel.mention(user)} 삭제됐습니다.`
    }
    protected async cmdAuth(context:SnowContext<BotConfig>, provider:string) {
        const { channel, message } = context
        const user = message.author
        const uid = {
            uid: user.id,
            platform: user.platform,
        }
        if (await this.userDB.get(uid, "mindaId") >= 0) {
            return "이미 인증되어 있습니다."
        }
        const credit = new MindaCredit(5000)
        const proves = await credit.getProviders()
        if (provider == null || proves.indexOf(provider) < 0) {
            return provider + "(이)라는 공급자가 없습니다." + "\n공급자 목록: " + proves.join(",")
        }
        const dm = await channel.dm(user)
        if (dm == null) {
            return "1:1 메시지를 보낼 수 없습니다." 
        }
        if (this.authQueue.has(user.getUID())) {
            return "이미 인증 과정 중입니다."
        }
        this.authQueue.set(user.getUID(), true)
        const url = await credit.genOAuth(provider)
        await dm.send(`${dm.mention(user)} ${url}`)
        credit.watchLogin()
        awaitEvent(credit.onLogin, 60000, async (token) => {
            const client = new MindaClient(token)
            await client.login()
            await this.userDB.set(uid, "mindaId", client.me.id)
            await dm.send(`${channel.mention(user)} 로그인 완료 (ID:${client.me.id})`)
            this.authQueue.delete(user.getUID())
        }).catch(async () => {
            await dm.send(`인증 시간이 초과됐습니다.`)
            this.authQueue.delete(user.getUID())
        })
        return null
    }
    protected async cmdRecordStat(context:SnowContext<BotConfig>, searchU:SnowUser) {
        const { channel, message } = context
        const user = searchU == null ? message.author : searchU
        const uid = {
            uid: user.id,
            platform: user.platform,
        }
        const getID = await this.userDB.get(uid, "mindaId")
        if (getID >= 0) {
            const query = await this.admin.searchRecords({
                user: getID,
            })
            const recs:string[] = []
            const getName = (u:MSUser) => {
                if (u != null && u.id >= 0) {
                    return u.username
                } else {
                    return "Unknown"
                }
            }
            for (let i = 0; i < query.length; i += 1) {
                const q = query[i]
                let out = ""
                const blackU = await this.admin.user(q.black)
                const whiteU = await this.admin.user(q.white)
                out += `[${i + 1}] ${getName(blackU)} vs `
                out += `${getName(whiteU)} (${q.loser === getID ? "패" : "승"})`
                recs.push(out)
            }
            if (recs.length <= 0) {
                await channel.send("없음")
            } else {
                await channel.send(recs.join("\n"))
            }
        } else {
            await channel.send("잘못된 유저입니다.")
        }
    }
}
@Entity()
class UserIdentifier {
    @PrimaryColumn()
    public uid:string
    @PrimaryColumn()
    public platform:string
}
@Entity()
class MindaID extends UserIdentifier {
    @Column("int8", {
        default: -1,
    })
    public mindaId:number
}