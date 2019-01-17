import { SnowPlugin } from "../snow/bot/snowcommand"
import SnowChannel from "../snow/snowchannel"
import SnowMessage from "../snow/snowmessage"

export default class Ping extends SnowPlugin {
    public async handleMessage(channel:SnowChannel, msg:SnowMessage) {
        const content = msg.content
        const pongCmd = await this.validateCommand("ping", channel, content)
        if (pongCmd != null) {
            await channel.send("Pong!")
        }
    }
}