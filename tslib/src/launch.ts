import chalk from "chalk"
import enquirer, { prompt } from "enquirer"
import { prompt as prompt2 } from "inquirer"
import { MindaAdmin, MindaClient, MindaCredit, MindaRoom } from "./index"
import { reqPost } from "./minda/mdrequest"
import { MSUser } from "./minda/structure/msuser"

async function login() {
    const credit = new MindaCredit(20000)
    const provs = await credit.getProviders()
    provs.push("debug")
    const selectProv = await prompt<{provider:string}>({
        type: "select",
        name: "provider",
        message: "oAuth 공급자를 선택해주세요.",
        choices: provs,
    })
    if (selectProv.provider === "debug") {
        return runCUI("WU7htx_4_helo4FO3Im44pU=")
    }
    console.log("인증을 시작합니다. 아래 링크를 눌러주세요.")
    const oAuth = await credit.genOAuth(selectProv.provider)
    console.log(chalk.yellow(oAuth))
    credit.watchLogin()
    credit.onLogin.one(async (token) => {
        console.log("Token: " + token)
        // runCUI(token)
        runTest(token)
    })
}
async function runTest(token:string) {
    const mindaC = new MindaClient(token)
    await mindaC.login()
    const changed = await mindaC.addSkin("test",
        "https://cdn.discordapp.com/attachments/152746825806381056/550229916621209600/kkinux.png")
    const profile = await mindaC.setProfile("알략", 
        "https://cdn.discordapp.com/attachments/152746825806381056/550229916621209600/kkinux.png")
    await mindaC.setSkin(changed)
    console.log(await mindaC.getProfile())
}
async function runCUI(token:string) {
    const cui = new CUI(token)
    await cui.start()
    while (true) {
        const room = await cui.selectRoom()
        await cui.hookRoom(room)
        await cui.render(room)
        await room.sendChat("Example Client")
        if (await cui.receiveCmd(room)) {
            break
        } else {
            continue
        }
    }
    process.exit(0)
}
class CUI {
    protected token:string
    protected client:MindaClient
    protected stack:string[]
    protected userCache:Map<number, MSUser> = new Map()
    public constructor(token:string) {
        this.token = token
    }
    public async start() {
        this.client = new MindaClient(this.token)
        const logined = await this.client.login()
        if (!logined) {
            console.log(chalk.red("잘못된 토큰입니다."))
            throw new Error("Wrong token")
        }
    }
    public async receiveCmd(room:MindaRoom) {
        while (true) {
            await this.render(room)
            let {cmd} = await prompt<{cmd:string}>({
                type: "input",
                name: "cmd",
                message: "명령어>",
            })
            if (cmd.startsWith(":")) {
                cmd = cmd.substr(1)
                if (cmd === "leave") {
                    room.close()
                    return false
                } else if (cmd === "exit") {
                    room.close()
                    return true
                } else if (cmd === "gg") {
                    await room.giveUp()
                }
            } else {
                await room.sendChat(cmd)
            }
        }
        return false
    }
    public async selectRoom() {
        while (true) {
            const rooms = await this.client.fetchRooms()
            const choices = rooms.map((v) => ({
                name: v.id,
                message: v.conf.name,
            }))
            choices.push({
                name: "/mkroom",
                message: "방 만들기",
            })
            choices.push({
                name: "/refresh",
                message: "목록 갱신",
            })
            const {choice} = await prompt<{ choice:string }>({
                type: "select",
                name: "choice",
                message: "방을 선택해 주세요.",
                choices,
            })
            if (choice === "/refresh") {
                continue
            }
            if (choice === "/mkroom") {
                const roomName = await prompt<{rn:string}>({
                    type: "input",
                    name: "rn",
                    message: "방 이름",
                })
                const pub = await prompt2<{bl:string[]}>({
                    type: "checkbox",
                    name: "bl",
                    message: "방 설정",
                    choices: ["공개"],
                })
                const usePub = pub.bl.indexOf("공개") >= 0
                const mkRoom = await this.client.createRoom(roomName.rn, usePub)
                return mkRoom
            }
            return this.client.joinRoom(rooms.find((v) => v.id === choice))
        }
    }
    public async hookRoom(room:MindaRoom) {
        this.stack = []
        room.onChat.sub(async (chat) => {
            this.stack.push(`${chalk.gray(`[채팅]`)} ${
                chalk.bold((await this.getUser(chat.user)).username)}: ${chat.content}`)
            this.render(room)
        })
        room.onBan.sub(async (user) => {
            this.stack.push(`${chalk.gray(`[알림]`)} ${
                chalk.bold((await this.getUser(user)).username)}님이 밴당했습니다.`)
            this.render(room)
        })
        room.onConf.sub(async () => {
            this.render(room)
        })
    }
    public async getUser(id:number) {
        if (this.userCache.has(id)) {
            return this.userCache.get(id)
        } else {
            const u = await this.client.user(id)
            this.userCache.set(id, u)
            return u
        }
    }
    public async render(room:MindaRoom) {
        // title
        process.stdout.write("\x1Bc")
        console.log(chalk.hex("#ffcd91")(` # ${room.name}`))
        console.log(chalk.bgBlack.white(` 흑) ${
            room.black >= 0 ? (await this.getUser(room.black)).username : "???"}`))
        console.log(chalk.bgWhite.black(` 백) ${
            room.white >= 0 ? (await this.getUser(room.white)).username : "???"}`))
        console.log(` 방장: ${(await this.getUser(room.owner)).username}`)
        const joins = await Promise.all(room.users.map((v) => this.getUser(v)))
        console.log(`유저 목록: ${joins.map((v) => v.username).join(", ")}`)
        console.log(`게임 진행중: ${room.ingame}`)
        console.log(chalk.bgYellow("".padStart(process.stdout.columns, " ")))
        const stacks = [...this.stack].splice(
            Math.max(0, this.stack.length - 8), Math.min(this.stack.length, 8)).join("\n")
        console.log(stacks)
    }
}
async function start(token:string) {


}
async function run() {
    const aClient = new MindaAdmin("WU7htx_4_helo4FO3Im44pU=")
    await aClient.login()

    const mkToken = async (name:string) => 
        (await aClient.createUser(name, false)).token
    const black = new MindaClient(await mkToken("dBlack"))
    await black.login()
    const white = new MindaClient(await mkToken("dWhite"))
    await white.login()
    const blackG = await black.createRoom("맞짱1")
    const whiteG = await white.joinRoom(blackG)
    blackG.onChat.sub((str) => {
        console.log("채팅: " + str.content)
    })
    const bl:boolean[] = []
    bl.push(blackG.sendChat("Black Ready"))
    bl.push(whiteG.sendChat("White Ready"))
    bl.push(await blackG.setBlack(black.me))
    bl.push(await blackG.setWhite(white.me))
    bl.push(await blackG.startGame())
    bl.push(await whiteG.giveUp())
    console.log(bl)
}
// runCUI("WU7htx_4_helo4FO3Im44pU=")
login()