import { SnowProvider } from "../provider/snowprovider"
import SnowChannel from "../snowchannel"
import SnowMessage from "../snowmessage"
import SnowUser from "../snowuser"

export default class SnowCommand<P extends Array<string | number | boolean | SnowUser>, R> {
    public readonly name:string
    public readonly help:string
    protected prefix:string
    protected exampleParam:P
    protected func:(context:SnowContext, ...args:P) => Promise<R>
    /**
     * Create Command Receiver
     * @param commandName Prefix of command
     * @param helpDesc Help Description
     * @param commander The Execute function
     * @param expParam ~~Example Parameter (require for type info)~~
     */
    public constructor(commandName:string,
        helpDesc:string, commander:(context:SnowContext, ...args:P) => Promise<R>) {
        this.name = commandName
        this.help = helpDesc
        this.func = commander
    }
    /*
    public bindProvider(prov:SnowProvider) {
        prov.onMessage.sub((ch, msg) => this.handleMessage(ch, msg))
        // prov.onReady.sub(() => this.handleReady())
        const test = new SnowCommand("Hello", "World!", (snow:SnowContext, str:string) => {
            return Promise.resolve()
        })
    }
    public abstract async handleMessage(channel:SnowChannel, message:SnowMessage):Promise<void>
    public async handleReady() {
        console.log("Ready!")
    }
    protected async validateCommand(cmds:string | string[], ch:SnowChannel, str:string | SnowMessage) {
        if (!Array.isArray(cmds)) {
            cmds = [cmds]
        }
        if (str instanceof SnowMessage) {
            str = str.content
        }
        const splitInput = str.split(/\s+/ig)
        for (const cmd of cmds) {
            if (str.startsWith(this.prefix + cmd)) {
                return {
                    command: cmd,
                    args: await ch.decodeArgs(splitInput.splice(0, 1)),
                }
                break
            }
        }
        return null
    }
    */
}
export interface SnowContext {
    channel:SnowChannel,
    message:SnowMessage,
}