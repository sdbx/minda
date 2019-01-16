import { SnowProvider } from "../provider/snowprovider"
import SnowChannel from "../snowchannel"
import SnowMessage from "../snowmessage"

export abstract class SnowPlugin {
    protected prefix:string
    public constructor(prefix:string) {
        this.prefix = prefix
    }
    public bindPlugin(prov:SnowProvider) {
        prov.onMessage.sub((ch, msg) => this.handleMessage(ch, msg))
        prov.onReady.sub(() => this.handleReady())
    }
    /**
     * When message is receive
     * @param channel Sent channel
     * @param message Message
     */
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
}