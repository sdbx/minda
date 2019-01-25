import * as Minda from "minda-ts"
import { MindaClient, MindaCredit } from "minda-ts"
import { EventDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import { Column, Entity } from "typeorm"
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
    snow.addCommand(new SnowCommand({
        name: "ping",
        func: async (context, arg1, arg2) => {
            const { channel, configChannel } = context
            if (arg1 === "get") {
                await channel.send("쪼리핑! " + configChannel.jjoa)
            } else if (arg1 === "set") {
                configChannel.jjoa = arg2
                // await context.updateChannelConfig()
            }
        },
        paramNames: ["쪼리", "핑"],
        description: "핑을 날립니다.",
    }, "string", "string"))
}

run2()

class AuthMinda {
    public onReady = new SimpleEventDispatcher<string>()
    protected credit = new MindaCredit()
    protected client:MindaClient
    protected subs:() => void
    public constructor() {
        this.onReady.sub(async (token) => {
            this.client = new MindaClient(token)
            await this.client.init()
            console.log(this.client.me)
        })
    }
    public async authMinda(context:SnowContext<{}>, provider:string) {
        const {channel, message} = context

    }
}