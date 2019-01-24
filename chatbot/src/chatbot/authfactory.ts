import { MindaAdmin, MindaCredit } from "minda-ts"
import path from "path"
import { Column, Entity } from "typeorm"
import SnowCommand, { SnowContext } from "../snow/bot/snowcommand"
import BaseGuildCfg from "../snow/config/baseguildcfg"
import SnowConfig from "../snow/config/snowconfig"
import awaitEvent from "../timeout"
import { bindFn } from "../util"
import BotConfig from "./guildcfg"

export default class AuthFactory {
    public commands:Array<SnowCommand<BotConfig>> = []
    protected dbpath:string
    protected db:SnowConfig<MindaID>
    public constructor(dir:string) {
        this.db = new SnowConfig(MindaID, path.resolve(dir, "mindaid.sqlite"))
        this.commands.push(new SnowCommand({
            name: "auth",
            paramNames: ["oAuth-공급자"],
            description: "민다 인-증을 해봅시다.",
            func: bindFn(this, this.cmdAuth),
        }, "string"))
    }
    public async init() {
        await this.db.connect()
    }
    protected async cmdAuth(context:SnowContext<BotConfig>, provider:string) {
        const { channel, message } = context
        const user = message.author
        const credit = new MindaCredit(5000)
        const proves = await credit.getProviders()
        if (proves.indexOf(provider) < 0) {
            await channel.send(provider + "(이)라는 공급자가 없습니다!" + "\n공급자 목록: " + proves.join(","))
            return
        }
        const url = await credit.genOAuth(provider)
        await channel.send("oAuth: " + url)
        credit.watchLogin()
        awaitEvent(credit.onLogin, 30000, async () => {
            await channel.send("로그인이 완료됐습니다 고갱님")
        })
    }
}
@Entity()
class MindaID extends BaseGuildCfg {
    @Column("int8", {
        default: -1,
    })
    public mindaId:number
}