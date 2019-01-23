import Discord from "discord.js"
import { EventDispatcher } from "strongly-typed-events"
import SnowChannel from "../snowchannel"
import SnowMessage from "../snowmessage"
import DiscordSnowCh, { messageToSnow } from "./discordsnowch"
import { SnowProvider } from "./snowprovider"
import SnowConfig from "../config/snowconfig";
import BaseGuildCfg from "../config/baseguildcfg";

export default class DiscordSnow<C extends BaseGuildCfg> extends SnowProvider<C> {
    protected token:string
    protected client:Discord.Client
    protected channels:Map<string, SnowChannel>
    public constructor(token:string, store:SnowConfig<C>) {
        super(store)
        this.token = token
        this.channels = new Map()
    }
    public async init() {
        await super.init()
        this.client = new Discord.Client()
        this.client.on("message", (m) => this.handleMessage(m))
        this.client.on("ready", () => this.onReady.dispatch())
        await this.client.login(this.token)
    }
    protected handleMessage(msg:Discord.Message) {
        const ch = msg.channel
        if (!this.channels.has(ch.id)) {
            this.channels.set(ch.id, new DiscordSnowCh(ch))
        }
        this.onMessage.dispatchAsync(this.channels.get(ch.id), messageToSnow(msg))
    }
}