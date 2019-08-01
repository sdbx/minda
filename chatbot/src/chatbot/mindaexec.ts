import { cLog } from "chocolog"
import { prompt } from "enquirer"
import { MindaClient, MindaCredit,
    MindaRoom, MSLoseCause, MSUser, MSRecStat } from "minda-ts"
import { Column, Entity, PrimaryColumn } from "typeorm"
import SnowCommand, { SnowContext } from "../snow/bot/snowcommand"
import SnowChannel from "../snow/channel/snowchannel"
import SimpleConfig from "../snow/config/simpleconfig"
import SnowUser from "../snow/snowuser"
import { subscribe } from "../timeout"
import { bindFn } from "../util"
import { WebpackTimer } from "../webpacktimer"
import { defaultBoard } from "./cbconst"
import BotConfig from "./guildcfg"
import path from "path"

export default class MindaExec {
    public commands:Array<SnowCommand<BotConfig>> = []
    protected client:MindaClient
    protected dbpath:string
    protected userDB:SimpleConfig<MindaID, UserIdentifier>
    protected authQueue:Map<string, boolean> = new Map()
    protected playingQueue:Map<string, MindaRoom> = new Map()
    public constructor(clientToken:string, dir:string) {
        this.userDB = new SimpleConfig(MindaID, path.resolve(dir, "mindaid.sqlite"))
        this.client = new MindaClient(clientToken)
        this.commands.push(new SnowCommand({
            name: "auth",
            paramNames: [],
            description: "Authenticate",
            func: bindFn(this, this.cmdAuth),
        }))
        this.commands.push(new SnowCommand({
            name: "unauth",
            paramNames: [],
            description: "Unauthenticate",
            func: bindFn(this, this.cmdUnAuth),
        }))
        // this.commands.push(new SnowCommand({
        //     name: "fight",
        //     paramNames: ["mention of user"],
        //     description: "Challange the user",
        //     func: bindFn(this, this.cmdFight),
        // }, "SnowUser"))
        this.commands.push(new SnowCommand({
            name: "stats",
            paramNames: ["mention of user", "page number"],
            description: "view match history of the user",
            func: bindFn(this, this.cmdRecordStat),
            reqLength: 0,
        }, "SnowUser", "number"))
        this.commands.push(new SnowCommand({
            name: "skin",
            paramNames: ["mention of user"],
            description: "show the current stone skin of the user",
            func: bindFn(this, this.cmdSkin),
            reqLength: 0,
        },"SnowUser"))
        this.commands.push(new SnowCommand({
            name: "syncprofile",
            paramNames: [],
            description: "use Discord profile image as Minda profile image",
            func: bindFn(this, this.cmdSyncProfile)
        }))
        this.commands.push(new SnowCommand({
            name: "rooms",
            paramNames: [],
            description: "list public Minda rooms",
            func: bindFn(this, this.cmdRoomList)
        }))
    }
    public async init() {
        await this.userDB.connect()
        return this.client.login()
    }
    public async genToken() {
        const credit = new MindaCredit(60000)
        const provs = await credit.getProviders()
        const {choice} = await prompt({
            type: "select",
            name: "choice",
            message: "Select provider",
            choices: provs,
        })
        const url = await credit.genOAuth(choice)
        cLog.i("oAuth URL", url)
        credit.watchLogin()
        const token = await subscribe(credit.onLogin, 1000)
        cLog.v("Token", token)
        this.client = new MindaClient(token)
        await this.client.login()
    }
    protected async cmdFight(context:SnowContext<BotConfig>, otherUser:SnowUser) {
        const { channel, message } = context
        const getMindaUser = async (user:SnowUser) => {
            const u = await this.userDB.get({
                uid: user.id,
                platform: user.platform,
            }, "mindaId")
            if (u < 0) {
                return null
            }
            return this.client.user(u)
        }
        const user1 = message.author
        const user2 = otherUser
        const minda1 = await getMindaUser(user1)
        const minda2 = this.client.me // await getMindaUser(user2)
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
        // set category force.. f***
        if (context.channel.provider === "discord" &&
            context.configGroup.manageCategory.length < 1) {
            const code = await channel.prompt(context.message.author, ["카테고리를 지정해주세요."])
            context.configGroup.manageCategory = code.content
        }
        let subCh:SnowChannel
        try {
            subCh = await channel.createChannel("Test", {
                category: context.configGroup.manageCategory,
            })
        } catch (err) {
            context.configGroup.manageCategory = ""
            channel.dm(message.author).then((v) => v.send(err.toString()))
            return `채널 생성에 실패했습니다.`
        }

        const room = await this.client.createRoom(`[${channel.name()}] ${user1.nickname} vs ${user2.nickname}`)
        if (!await new Promise<boolean>((res, rej) => {
            let fetchTry = 0
            const fetchId = WebpackTimer.setInterval(async () => {
                const roomFind = (await this.client.fetchRooms()).find((v) => v.id === room.id)
                if (roomFind != null) {
                    res(true)
                }
                if (fetchTry >= 5) {
                    WebpackTimer.clearInterval(fetchId)
                    res(false)
                }
                fetchTry += 1
            }, 500)
        })) {
            return `방 생성에 실패했습니다.`
        }
        await channel.send("방 이름: " + room.conf.name)
        /**
         * Debug
         */
        await room.setWhite(minda2)

        this.playingQueue.set(user1.getUID(), room)
        this.playingQueue.set(user2.getUID(), room)
        const deleteQueue = async () => {
            this.playingQueue.delete(user1.getUID())
            this.playingQueue.delete(user2.getUID())
            room.close()
            await subCh.deleteChannel()
        }

        room.onChat.sub(async (ch) => {
            const n = await this.client.user(ch.user).then((v) => v.username)
            subCh.send(`${n} : ${ch.content}`)
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
                winner = (await this.client.user(room.white)).username
                color = "하얀 돌"
            } else {
                winner = (await this.client.user(room.black)).username
                color = "검은 돌"
            }
            await subCh.send(`${winner} (${color}) 승리!`)
            await deleteQueue()
        })

        subscribe(room.onEnter, 60000).then(async (info) => {
            if (info.user === minda1.id) {
                if (await room.setBlack(minda1)) {
                    await room.sendChat(`흑돌 선수 ${minda1.username}님이 입장합니다.`);
                }
                else {
                    subCh.send("흑돌을 설정하는데 실패했습니다.");
                }
            } else if (info.user === minda2.id) {
                if (await room.setWhite(minda2)) {
                    await room.sendChat(`백돌 선수 ${minda2.username}님이 입장합니다.`);
                }
                else {
                    subCh.send("백돌을 설정하는데 실패했습니다.");
                }
            }
            if (room.black >= 0 && room.white >= 0) {
                if (await room.startGame()) {
                    await room.sendChat("경기를 시작합니다.");
                }
            }
        }).catch(async () => {
            await channel.send("유저가 접속을 안하여 방이 닫혔습니다.")
            await deleteQueue()
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
    protected async cmdRoomList(context:SnowContext<BotConfig>) {
        const { channel, message } = context
        const rooms = (await this.client.fetchRooms())
        let out = ""
        for (let i = 0; i < rooms.length; i += 1) {
            out += `[${i}] ${rooms[i].conf.name} - ${(await this.client.user(rooms[i].conf.king)).username}\n`
        }
        if (out.length < 1) {
            out = "없음"
        }
        return out
    }
    protected async cmdAuth(context:SnowContext<BotConfig>) {
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
        const dm = await channel.dm(user)
        if (dm == null) {
            return "1:1 메시지를 보낼 수 없습니다." 
        }
        if (this.authQueue.has(user.getUID())) {
            return "이미 인증 과정 중입니다."
        }
        this.authQueue.set(user.getUID(), true)
        const url = await credit.genOAuth("steam")
        await dm.send(`${dm.mention(user)} ${url}`)
        credit.watchLogin()
        subscribe(credit.onLogin, 60000).then(async (token) => {
            const client = new MindaClient(token);
            await client.login();
            await this.userDB.set(uid, "mindaId", client.me.id);
            await dm.send(`${channel.mention(user)} 로그인 완료 (ID:${client.me.id})`);
            this.authQueue.delete(user.getUID());
        }).catch(async () => {
            await dm.send(`인증 시간이 초과됐습니다.`)
            this.authQueue.delete(user.getUID())
        })
        return null
    }
    protected async cmdRecordStat(context:SnowContext<BotConfig>, page?:number, searchU?:SnowUser) {
        const { channel, message } = context
        const user = searchU == null ? message.author : searchU
        if (page == null || page < 1) {
            page = 1
        }
        const uid = {
            uid: user.id,
            platform: user.platform,
        }
        const getID = await this.userDB.get(uid, "mindaId")
        if (getID >= 0) {
            const records:MSRecStat[] = []
            for (let i = page; true; i += 1) {
                const query = await this.client.searchRecords({
                    user: getID,
                    p: i,
                })
                if (query != null) {
                    records.push(...query)
                    if (query.length < 10) {
                        break
                    }
                } else {
                    break
                }
                cLog.v("Parsing", i)
                break
            }
            const angry = "\u{1F621}"
            const smile = "\u{1F60F}"
            const blackE = "\u{26AB}"
            const whiteE = "\u{26AA}" 
            const recs:string[] = []
            const getName = (u:MSUser) => {
                if (u != null && u.id >= 0) {
                    if (u.id === getID) {
                        return `**${u.username}**`
                    } else {
                        return `${u.username}`
                    }
                } else {
                    return "Unknown"
                }
            }
            const uInfo:Map<number, MSUser> = new Map()
            const getU = async (id:number) => {
                if (uInfo.has(id)) {
                    return uInfo.get(id)
                } else {
                    const u = await this.client.user(id)
                    uInfo.set(id, u)
                    return u
                }
            }
            let loseStack = 0
            records.sort((a, b) => b.created_at - a.created_at)
            for (let i = 0; i < records.length; i += 1) {
                const q = records[i]
                let out = ""
                const blackU = await getU(q.black)
                const whiteU = await getU(q.white)
                const isBlack = blackU.id === getID
                const lose = q.loser === getID
                if (lose) {
                    loseStack += 1
                }
                let loseCause = ""
                switch (q.cause) {
                    case MSLoseCause.gg: {
                        loseCause = "항복"
                    } break
                    case MSLoseCause.lostStones: {
                        loseCause = "실력"
                    } break
                    case MSLoseCause.timeout: {
                        loseCause = "시간오버"
                    } break
                }
                out += `[${i + 1}] vs ${getName(isBlack ? whiteU : blackU)} ${
                    isBlack ? blackE : whiteE} ${lose ? angry : smile}`
                out += ` (${loseCause}, \u{1F552}${this.timeToString(q.created_at)})`
                recs.push(out)
            }
            if (recs.length <= 0) {
                await channel.send("없음")
            } else {
                const skinInfo = await this.client.getSkinOfUser(getID)
                const winPercent = `${Math.round((records.length - loseStack) * 1000 / records.length) / 10}%`
                recs.unshift(`[스킨] ${skinInfo == null ? "없음" : skinInfo.name}\n[승률] ${winPercent}`)
                await channel.send(recs.join("\n"))
            }
        } else {
            await channel.send("잘못된 유저입니다.")
        }
    }
    protected async cmdSyncProfile(context:SnowContext<BotConfig>) {
        const { channel, message } = context
        const user = message.author
        const dm = await channel.dm(user)
        
        const token = new MindaCredit(60000)
        let client:MindaClient
        await dm.send("로그인을 해주세요.\n" + await token.genOAuth("discord"))
        token.watchLogin()
        try {
            client = new MindaClient(await subscribe(token.onLogin, 1000))
            const modifyU = await client.setProfile(user.nickname, user.profileImage)
            return `별명: ${modifyU.username}\n사진: ${modifyU.picture}`
        } catch (err) {
            console.error(err)
            return "시간이 초과되었습니다."
        }
        return null
    }
    protected async cmdSkin(context:SnowContext<BotConfig>, searchU:SnowUser) {
        const { channel, message } = context
        const user = searchU == null ? message.author : searchU
        const uid = {
            uid: user.id,
            platform: user.platform,
        }
        const id = await this.userDB.get(uid, "mindaId")
        if (id >= 0) {
            const skin = await this.client.getSkinOfUser(id)
            cLog.v("SkinID", skin.id)
            if (skin != null && skin.id != null && skin.id >= 0) {
                await context.channel.send("검은 돌", skin.black_picture)
                await context.channel.send("하얀 돌", skin.white_picture)
            } else {
                await context.channel.send("스킨 없음")
            }
        } else {
            await context.channel.send("No user found.")
        }
    }
    private async getMindaUser(user:SnowUser) {
        const getID = await this.userDB.get({
            uid: user.id,
            platform: user.platform,
        }, "mindaId")
        if (getID >= 0) {
            const u:MSUser = await this.client.user(getID).catch((v) => null)
            if (u != null) {
                return u
            }
        }
        return null
    }
    private timeToString(time:number) {
        const date = new Date(time)
        const isPM = date.getHours() >= 12
        const padZero = (n:number, ln:number) => n.toString().padStart(ln, "0")
        const a = `${padZero(date.getMonth() + 1, 2)}/${padZero(date.getDate(), 2)} ` +
            `${isPM ? "PM" : "AM"}${date.getHours() % 12 === 0 ? 12 : date.getHours() % 12}` +
            `:${padZero(date.getMinutes(), 2)}:${padZero(date.getSeconds(), 2)}`
        return a
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