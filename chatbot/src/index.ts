import { registerFont } from "canvas"
import { cLog } from "chocolog"
import fs from "fs-extra"
import { MindaClient, MindaCredit, MSGrid } from "minda-ts"
import * as Minda from "minda-ts"
import { EventDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import { Column, Entity } from "typeorm"
import { renderBoard } from "./chatbot/boardrender"
import GlobalConfig from "./chatbot/globalcfg"
import BotConfig from "./chatbot/guildcfg"
import MindaExec from "./chatbot/mindaexec"
import SnowCommand, { SnowContext } from "./snow/bot/snowcommand"
import BaseGuildCfg from "./snow/config/baseguildcfg"
import JsonConfig from "./snow/config/jsonconfig"
import SnowConfig, { debugPath } from "./snow/config/snowconfig"
import TokenStore from "./snow/config/tokenstore"
import DiscordSnow from "./snow/provider/discordsnow"
import Snow from "./snow/snow"
import { Serializable, Serializify } from "./types/serializable"
import { bindFn } from "./util"

async function run2() {
    const tokenStore = new JsonConfig(GlobalConfig, `${debugPath}/config/token.json5`).ro
    const snow = new Snow(tokenStore, `${debugPath}/config`, BotConfig)
    const authF = new MindaExec(tokenStore.minda, `${debugPath}/config`)
    if (!await authF.init()) {
        await authF.genToken()
    }
    await cLog.v(await snow.login())
    await cLog.v(process.cwd())

    snow.addCommands(authF.commands)
    snow.addCommand(new SnowCommand({
        name: "ping",
        description: "핑핑 조리핑",
        paramNames: [],
        func: async (ctx) => {
            try {
                const m = await ctx.channel.prompt(ctx.message.author, ["당신은 바부입니까?"], 20000)
                await ctx.channel.send("바부는 " + m.content + "라고 말했습니다.")
            } catch {
                await ctx.channel.send("이런 달아났습니다.")
            }
        }
    }))
}
async function run3() {
    // tslint:disable-next-line
    const map = "0@0@0@0@0@0@0@2@2#0@0@0@0@0@0@0@2@2#0@0@0@0@0@0@2@2@2#0@1@0@0@0@0@2@2@2#1@1@1@0@0@0@2@2@2#1@1@1@0@0@0@0@2@0#1@1@1@0@0@0@0@0@0#1@1@0@0@0@0@0@0@0#1@1@0@0@0@0@0@0@0"
    const board = await renderBoard(new MSGrid(map),{
        black: "https://cdn.discordapp.com/emojis/471277517823803412.png?v=1",
        white: "https://cdn.discordapp.com/emojis/506470672873160734.png?v=1",
    }, {
        black: {username:"Black", stone: 2},
        white: {username:"White", stone: 8},
        maxstone: 9,
    })
    await fs.writeFile(`${debugPath}/config/test.png`, board)
}
// run3()
run2()
// console.log(debugPath)

class AuthMinda {
    public onReady = new SimpleEventDispatcher<string>()
    protected credit = new MindaCredit()
    protected client:MindaClient
    protected subs:() => void
    public constructor() {
        this.onReady.sub(async (token) => {
            this.client = new MindaClient(token)
            await this.client.login()
            cLog.i("Myself", this.client.me)
        })
    }
    public async authMinda(context:SnowContext<{}>, provider:string) {
        const {channel, message} = context

    }
}
interface SerialTest {
    id:number,
    username:string,
    // nullable
    picture?:number,
    permission:Minda.MSPerm,
    inventory:boolean,
}