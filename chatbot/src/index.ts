import * as Minda from "minda-ts"
import { MindaClient, MindaCredit } from "minda-ts"
import { EventDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import SnowCommand, { SnowContext } from "./snow/bot/snowcommand"
import SnowConfig from "./snow/config/snowconfig"
import DiscordSnow from "./snow/provider/discordsnow"
import { bindFn } from "./util"

async function run() {
    const tokens = await SnowConfig.getTokens()
    if (tokens["discord"] != null) {
        const t = tokens["discord"]
        const snowD = new DiscordSnow(t)
        await snowD.init()
        const authCmd = new AuthMinda()
        snowD.addCommand(
            new SnowCommand("ping", async (context, arg1, arg2) => {
                const {channel, message} = context
                await channel.send("쪼리핑!")
            }, "string", "number")
                .withRequires(2)
                .withHelp("핑핑 쪼리핑을 날립니다", "시간", "날짜")
        )
        snowD.addCommand(
            new SnowCommand("authminda", bindFn(authCmd, "authMinda"), "string").withHelp("인증", "공급자")
        )
    }
}

run()

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
    public async authMinda(context:SnowContext, provider:string) {
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