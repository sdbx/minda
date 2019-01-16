import { EventDispatcher, SignalDispatcher } from "strongly-typed-events"
import SnowCommand from "../bot/snowcommand"
import SnowChannel from "../snowchannel"
import SnowMessage from "../snowmessage"

export abstract class SnowProvider {
    public readonly onMessage = new EventDispatcher<SnowChannel, SnowMessage>() 
    public readonly onReady:SignalDispatcher = new SignalDispatcher()
    protected token:string
    protected commands:Array<SnowCommand<any[], any>> = []
    public async init() {
        this.onMessage.sub((ch, msg) => this.parseCommand(ch, msg))
    }
    public addCommand() {
        
    }
    protected async parseCommand(channel:SnowChannel, msg:SnowMessage) {
        const splitContents = msg.content.match(/[^\s"]+|"([^"]*)"/ig)
        if (splitContents == null) {
            return
        }
        const commandName = splitContents[0]
        for (let i = 0; i < splitContents.length; i += 1) {
            const content = splitContents[i]
            if (content.startsWith(`"`) && content.endsWith(`"`)) {
                splitContents[i] = content.substring(1, content.length - 1)
            }
        }
        const typedContents = await channel.decodeArgs(splitContents).then((v) => v.map((str) => {
            if (typeof str === "string") {
                if (/^\d+{1,16}$/i.test(str)) {
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
    }
}
