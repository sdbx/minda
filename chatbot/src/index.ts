import { registerFont } from "canvas"
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
import { bindFn } from "./util"

async function run2() {
    const tokenStore = new JsonConfig(GlobalConfig, `${debugPath}/config/token.json5`).ro
    const snow = new Snow(tokenStore, `${debugPath}/config`, BotConfig)
    const authF = new MindaExec(tokenStore.minda, `${debugPath}/config`)
    await authF.init()
    console.log(await snow.login())
    snow.addCommands(authF.commands)
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
run3()
// run2()
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
            console.log(this.client.me)
        })
    }
    public async authMinda(context:SnowContext<{}>, provider:string) {
        const {channel, message} = context

    }
}