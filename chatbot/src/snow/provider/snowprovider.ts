import { EventDispatcher, SignalDispatcher } from "strongly-typed-events"
import SnowCommand, { SnowContext, ParamDecodeAll, AllowDecode } from "../bot/snowcommand"
import SnowChannel from "../snowchannel"
import SnowMessage from "../snowmessage"
import SnowConfig, { SnowSchema, SnowConfigSimple } from "../config/snowconfig";
import BaseGuildCfg from "../config/baseguildcfg";
type ArgumentsType<T> = T extends (...args: infer A) => any ? A : never;

export abstract class SnowProvider<C extends BaseGuildCfg> {
    public readonly onMessage = new EventDispatcher<SnowChannel, SnowMessage>() 
    public readonly onReady:SignalDispatcher = new SignalDispatcher()
    protected store:SnowConfig<C>
    protected prefix:string = "!"
    protected commands:Array<SnowCommand<SnowSchema<C>, any[], any>> = []
    /**
     * Create now Provider with info
     * @param token Auth token for service provider
     * @param store Config store to use this channels.
     */
    public constructor(store:SnowConfig<C>) {
        this.store = store
    }
    public async init() {
        this.onMessage.sub((ch, msg) => this.parseCommand(ch, msg))
    }
    public createCommand<P extends AllowDecode[], R>(
        commandName: string,
        commander: (context: SnowContext<C>, ...args: ParamDecodeAll<P>) => R | Promise<R>,
        ...typeInfo: P) {
        return new SnowCommand(commandName, commander, ...typeInfo)
    }
    public addCommand(command:SnowCommand<SnowSchema<C>, any[], any>) {
        this.commands.push(command)
    }
    protected async parseCommand(channel:SnowChannel, msg:SnowMessage) {
        if (!msg.content.startsWith(this.prefix)) {
            return
        }
        const splitContents = msg.content.match(/[^\s"]+|"([^"]*)"/ig)
        if (splitContents == null) {
            return
        }
        const commandName = splitContents[0].substr(1)
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
        for (const command of this.commands) {
            if (command.name === commandName && command.checkParam(typedContents)) {
                await command.execute({
                    channel,
                    message: msg,
                    configChannel: chConfig,
                    configGroup: gpConfig,
                    updateGroupConfig: () => this.store.setConfig(gpConfig, channel.groupId, provider),
                    updateChannelConfig: () => this.store.setConfig(chConfig, channel.id, provider),
                }, typedContents)
            }
        }
    }
}
