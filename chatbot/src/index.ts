import * as Minda from "minda-ts"
import { MindaClient, MindaCredit } from "minda-ts"
import { EventDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import { Column, Entity } from "typeorm"
import SnowCommand, { SnowContext } from "./snow/bot/snowcommand"
import BaseGuildCfg from "./snow/config/baseguildcfg"
import SnowConfig, { debugPath } from "./snow/config/snowconfig"
import DiscordSnow from "./snow/provider/discordsnow"
import { bindFn } from "./util"

async function run2() {
    const tokens = await SnowConfig.getTokens()
    if (tokens["discord"] != null) {
        const t = tokens["discord"]
        const config = new SnowConfig(TestSchema,
            `${debugPath}/config/discord.sqlite`)
        await config.connect()
        const snowD = new DiscordSnow(t, config)
        await snowD.init()
        const authCmd = new AuthMinda()

        const cmd = snowD.createCommand("ping", async (context, arg1, arg2) => {
            const {channel, configChannel} = context
            if (arg1 === "get") {
                await channel.send("쪼리핑! " + configChannel.jjoa)
            } else if (arg1 === "set") {
                configChannel.jjoa = arg2
                await context.updateChannelConfig()
            }
        }, "string", "string")
            .withRequires(2)
            .withHelp("핑핑 쪼리핑을 날립니다", "시간", "날짜")
        snowD.addCommand(cmd)
        snowD.addCommand(
            new SnowCommand("authminda", bindFn(authCmd, "authMinda"), "string").withHelp("인증", "공급자")
        )
    }
}
async function run1() {
    const config = new SnowConfig(TestSchema,
        `${debugPath}/config/guild.sqlite`)
    await config.connect()
    const cfg = await config.getConfig("2018", "discord")
    console.log(JSON.stringify(cfg, null, 2))
    cfg.babu = false
    cfg.jjoa = "Dukjjji"
    cfg.kkiro = 53
    await config.setConfig(cfg, "2015", "discord")
}

@Entity()
class TestSchema extends BaseGuildCfg {
    @Column("boolean", {
        default: false,
    })
    public babu:boolean
    @Column({
        default: "dontlike",
    })
    public jjoa:string
    @Column("bigint", {
        default: 53,
    })
    public kkiro:number
    @Column({
        default: "death",
    })
    public deasu:string
    @Column({
        default: "IhateKkiro",
    })
    public iLikeKkiro:string
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
        const proves = await this.credit.getProviders()
        if (proves.indexOf(provider) < 0) {
            await channel.send(provider + "(이)라는 공급자가 없습니다!" + "\n공급자 목록: " + proves.join(","))
            return            
        }
        const url = await this.credit.genOAuth(provider)
        await channel.send("oAuth: " + url)
        this.credit.watchLogin()
        if (this.subs != null) {
            this.subs()
        }
        this.subs = this.credit.onLogin.one((token) => {
            channel.send("인증 완료!")
            this.onReady.dispatch(token)
        })
    }
}