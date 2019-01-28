import { DeepReadonly } from "minda-ts/build/main/types/deepreadonly"
import path from "path"
import { Serializify } from "../types/serializable"
import { bindFn } from "../util"
import SnowCommand, { AllowDecode, AvaiableParams, ParamDecodeAll, SnowContext } from "./bot/snowcommand"
import BaseGuildCfg from "./config/baseguildcfg"
import SnowConfig from "./config/snowconfig"
import TokenStore from "./config/tokenstore"
import DiscordSnow from "./provider/discordsnow"
import { SnowProvider } from "./provider/snowprovider"

export default class Snow<C extends BaseGuildCfg> {
    /**
     * Login info of services
     */
    protected tokens:DeepReadonly<TokenStore & ServicePair>
    /**
     * Config of this bot.
     */
    protected config:SnowConfig<C>
    /**
     * Directory to save configs
     */
    protected configDir:string
    /**
     * Providers
     */
    protected services:Partial<Services<C>> = {}
    protected commands:Array<SnowCommand<C>> = []
    public constructor(store:TokenStore & ServicePair, dir:string, schema:new () => C) {
        this.tokens = store
        this.configDir = path.resolve(dir)
        this.config = new SnowConfig(schema, path.resolve(dir, "snow.sqlite"))
    }
    /**
     * Add command to snow runtime
     * @param command Command
     */
    public addCommand(command:SnowCommand<C>) {
        this.commands.push(command)
    }
    /**
     * Add command**s**
     * @param commands Commands
     */
    public addCommands(commands:Array<SnowCommand<C>>) {
        for (const c of commands) {
            this.commands.push(c)
        }
    }
    /**
     * Remove command from snow runtime
     * @param cmdNames Command name
     */
    public deleteCommand(...cmdNames:string[]) {
        for (const cmdN of cmdNames) {
            const i = this.commands.findIndex((v) => v.name === cmdN)
            if (i >= 0) {
                this.commands.splice(i, 1)
            }
        }
    }
    /**
     * Login services via config
     */
    public async login() {
        await this.config.connect()
        const result:{[key in keyof Services<any>]: boolean} = {
            discord: false,
            ncc: false,
        }
        if (this.tokens.discord.length >= 1) {
            const discordP = new DiscordSnow(this.tokens.discord, this.config)
            this.services.discord = discordP
        }
        if (this.tokens.ncc.length >= 1) {
            // todo :)
            // this.services.ncc = nccP
        }
        for (const [key, service] of Object.entries(this.services)) {
            try {
                await service.init()
                service.hookHandle(bindFn(this, this.handleCommands))
                result[key] = true
            } catch (err) {
                console.error(err)
                delete this.services[key]
                result[key] = false
            }
        }
        return result
    }
    protected hasService(name:keyof Services<C>) {
        return this.services[name] != null
    }
    /**
     * Handle Command.. just like internal
     * @param commandName Command's name
     * @param context Context to send
     * @param params Parameters of receive
     */
    protected async handleCommands(commandName:string, context:SnowContext<C>, params:AvaiableParams[]) {
        const exec = this.commands.find(
            (command) => command.name === commandName && command.checkParam(params))
        if (exec != null) {
            const r = await exec.execute(context, params)
            switch (typeof r) {
                case "string":
                case "number":
                case "bigint":
                case "boolean": {
                    await context.channel.send(r.toString())
                } break
            }
        }
    }
}
type ServicePair = {
    [key in keyof Services<any>]:string | number | object
}
interface Services<C extends BaseGuildCfg> {
    discord:DiscordSnow<C>,
    ncc:SnowProvider<C>,
}