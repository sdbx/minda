import { EventDispatcher, SignalDispatcher } from "strongly-typed-events"
import SnowCommand, { AllowDecode, AvaiableParams, ParamDecodeAll, SnowContext } from "../bot/snowcommand"
import SnowChannel from "../channel/snowchannel"
import BaseGuildCfg from "../config/baseguildcfg"
import SnowConfig, { SnowConfigSimple, SnowSchema } from "../config/snowconfig"
import SnowMessage from "../snowmessage"

type ArgumentsType<T> = T extends (...args:infer A) => any ? A : never

export abstract class SnowProvider<C extends BaseGuildCfg> {
    public readonly onMessage = new EventDispatcher<SnowChannel, SnowMessage>() 
    public readonly onReady:SignalDispatcher = new SignalDispatcher()
    /**
     * Logined?
     */
    public available:boolean = false
    public abstract readonly name:string
    protected store:SnowConfig<C>
    protected prefix:string = "??"
    protected commands:Array<SnowCommand<C>> = []
    protected handleFn:(commandName:string, context:SnowContext<C>, params:AvaiableParams[]) => Promise<void>
    /**
     * Create now Provider with info
     * @param token Auth token for service provider
     * @param store Config store to use this channels.
     */
    public constructor(store:SnowConfig<C>) {
        this.store = store
        this.onMessage.sub((ch, msg) => this.parseCommand(ch, msg))
        this.onReady.one(() => this.available = true)
    }
    /**
     * Initialize Provider
     * 
     * Login, etc...
     */
    public abstract init():Promise<void>
    /**
     * Add command to internal handler
     * @param command Command
     */
    public addCommand<P extends any[]>(command:SnowCommand<C, P>) {
        this.commands.push(command)
        return command
    }
    /**
     * Hook this commandHandler to invoke `fn`
     * @param fn Function
     */
    public hookHandle(fn:(commandName:string, context:SnowContext<C>, params:AvaiableParams[]) => Promise<void>) {
        this.handleFn = fn
    }
    /**
     * Parse Message to make command
     * @param channel Channel
     * @param msg Message
     */
    protected async parseCommand(channel:SnowChannel, msg:SnowMessage) {
        channel.onMessage.dispatch(channel, msg)
        if (!msg.content.startsWith(this.prefix)) {
            return
        }
        const splitContents = msg.content.match(/[^\s"]+|"([^"]*)"/ig)
        if (splitContents == null) {
            return
        }
        const commandName = splitContents[0].substr(this.prefix.length)
        splitContents.splice(0, 1)
        for (let i = 0; i < splitContents.length; i += 1) {
            const content = splitContents[i]
            if (content.startsWith(`"`) && content.endsWith(`"`)) {
                splitContents[i] = content.substring(1, content.length - 1)
            }
        }
        const typedContents = await channel.decodeArgs(splitContents).then((v) => v.map((str) => {
            if (typeof str === "string") {
                if (/^\d{1,16}$/i.test(str)) {
                    return Number.parseInt(str)
                } else if (str === "true" || str === "false") {
                    return str === "true"
                } else {
                    return str
                }
            } else {
                return str
            }
        }))
        const provider = channel.provider
        const chConfig = await this.store.getConfig(channel.id, provider)
        const gpConfig = await this.store.getConfig(channel.groupId, provider)
        const fn = this.handleFn == null ? this.handleCommands : this.handleFn
        await fn(commandName, {
            channel,
            message: msg,
            configChannel: chConfig,
            configGroup: gpConfig,
            updateGroupConfig: () => this.store.setConfig(gpConfig, channel.groupId, provider),
            updateChannelConfig: () => this.store.setConfig(chConfig, channel.id, provider),
        }, typedContents)
    }
    /**
     * Internal command handler
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
                // case "bigint":
                case "boolean": {
                    await context.channel.send(r.toString())
                } break
            }
        }
    }
}
