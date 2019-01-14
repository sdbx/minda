import Discord from "discord.js"
import SnowChannel, { ConfigDepth } from "../snowchannel"
import SnowMessage from "../snowmessage"
import SnowUser from "../snowuser"
import { getFirst } from "../snowutil"

export default class DiscordSnowCh extends SnowChannel {
    public provider = "discord"
    public supportFile = true
    protected channel:Discord.TextChannel | Discord.DMChannel
    public constructor(channel:Discord.TextChannel | Discord.DMChannel) {
        super()
        this.channel = channel
    }
    public async send(text:string, image?:string | Buffer):Promise<SnowMessage> {
        return this._send(text, image == null ? [] : [image])
    }
    public async sendFiles(files:Array<string | Buffer>, text?:string):Promise<SnowMessage> {
        return this._send(text, files)
    }
    public async getConfig(depth:ConfigDepth, key:string):Promise<unknown> {
        throw new Error("Method not implemented.")
    }
    protected async _send(text:string, files:Array<string | Buffer>) {
        if (this.channel instanceof Discord.TextChannel) {
            const sendable = this.channel.permissionsFor(this.channel.client.user).has("SEND_MESSAGES")
            if (!sendable) {
                return null
            }
        }
        const message = getFirst(await this.channel.send(text, {
            files,
        }))
        if (message != null) {
            return messageToSnow(message)
        }
        return null
    }
}
/**
 * Discord Message -> SnowMessage (Partial)
 * @param msg Discord Message
 */
export function messageToSnow(msg:Discord.Message) {
    const images:string[] = []
    const files:string[] = []
    if (msg.attachments.size >= 1) {
        for (const attach of  msg.attachments.array()) {
            if (attach.width >= 1 && attach.height >= 1) {
                images.push(attach.url)
            } else {
                files.push(attach.url)
            }
        }
    }
    const snowM = new SnowMessage()
    snowM.content = msg.content
    snowM.fields = []
    if (images.length >= 1) {
        snowM.image = images.splice(0, 1)[0]
        if (images.length >= 1) {
            for (const i of images) {
                snowM.fields.push({
                    type: "image",
                    data: i, 
                })
            }
        }
    }
    if (files.length >= 1) {
        for (const f of files) {
            snowM.fields.push({
                type: "file",
                data: f,
            })
        }
    }
    snowM.id = BigInt(msg.id)
    snowM.author = userToSnow(msg.member == null ? msg.author : msg.member)
    return snowM
}
/**
 * Discord user -> SnowUser
 * @param user User
 */
export function userToSnow(user:Discord.GuildMember | Discord.User) {
    const snowU = new SnowUser()
    snowU.id = user.id
    if (user instanceof Discord.GuildMember) {
        snowU.nickname = user.nickname == null ? user.user.username : user.nickname
    } else {
        snowU.nickname = user.username
    }
    snowU.platform = "discord"
    const uImage = (u:Discord.User) => {
        if (u.displayAvatarURL != null) {
            return u.displayAvatarURL
        } else {
            return u.defaultAvatarURL
        }
    }
    snowU.profileImage = uImage((user instanceof Discord.GuildMember) ? user.user : user)
    return snowU
}