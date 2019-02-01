import { EventDispatcher } from "strongly-typed-events"
import { WebpackTimer } from "../../webpacktimer"
import { GidType } from "../config/baseguildcfg"
import { SnowErrors } from "../snowerror"
import SnowMessage from "../snowmessage"
import { SnowPerm } from "../snowperm"
import SnowUser from "../snowuser"

export default abstract class SnowChannel {
    /**
     * Event: on Message received from channel
     * 
     * Not include "prefix" command.
     */
    public readonly onMessage = new EventDispatcher<SnowChannel, SnowMessage>()
    /**
     * Provider of Channel (Ex. discord, ncc)
     */
    public abstract readonly provider:string
    /**
     * Does this provider support sending & receiving file?
     */
    public abstract readonly supportFile:boolean
    /**
     * Channel of id (Ex. 10007122)
     */
    public abstract readonly id:GidType
    /**
     * Parent of channel's id
     */
    public abstract readonly groupId:GidType
    /**
     * Send Text or Image (or both if support.)
     * @param text Text of message
     * @param image Image of message
     * @returns sended message if success, null if fail (not Promise.reject!)
     */
    public abstract async send(text:string, image?:string | Buffer):Promise<SnowMessage>
    /**
     * Send File (with text)
     * 
     * Please check `supportFile` value.
     * @param files File URL or Buffer []
     * @param text Additional Text to send.
     * @returns sended message if success, null if fail (not Promise.reject!)
     */
    public abstract async sendFiles(files:Array<Buffer | string>, text?:string):Promise<SnowMessage>
    /**
     * Get user from id
     * @param id ID
     */
    public async user(id:string):Promise<SnowUser> {
        const list = await this.userList()
        return list.find((v) => v.id === id)
    }
    /**
     * Get all users from this channel
     */
    public abstract async userList():Promise<SnowUser[]>
    /**
     * Get Permisson from this channel for me
     * 
     * TODO.
     */
    public abstract async permissions(user?:string | SnowUser):Promise<SnowPerm>
    /**
     * Get direct-message channel for the user
     * @param user User ID
     */
    public abstract async dm(user:string | SnowUser):Promise<SnowChannel>
    /**
     * Mention user (if support.. anyway?)
     * @param user User ID
     */
    public abstract mention(user:string | SnowUser):string
    /**
     * This channel's name
     */
    public abstract name():string
    /**
     * Prompt to receive response.
     * @param user To prompt user
     * @param ask Ask things
     * @param timeout Wait until time
     * @returns Message or Error (reject)
     */
    public async prompt(user:string | SnowUser, ask:[string, (string | Buffer)?], timeout = 30000) {
        const uid = typeof user === "string" ? user : user.id
        const req = await this.send(`${this.mention(user)} ${ask[0]}`, ask[1])
        if (req == null) {
            throw new Error(SnowErrors.cannotSend)
        }
        return new Promise<SnowMessage>((res, rej) => {
            let fn:() => void
            const timer = WebpackTimer.setTimeout(() => {
                fn()
                rej(new Error(SnowErrors.timeout))
            }, timeout)
            fn = this.onMessage.sub((ch, msg) => {
                if (msg.author.id === uid) {
                    fn()
                    res(msg)
                }
            })
        })
    }
    /**
     * Try to create channel
     * 
     * It's nearly impossible to create channel via multi-platform.
     * 
     * This method is HEAVY and experimental.
     * 
     * Error handling for debug.
     * @param name Channel Name
     * @returns SnowChannel or null (resolve)
     */
    public abstract async createChannel(name:string, options:CreateChannelOpts):Promise<SnowChannel>
    /**
     * Try to remove this channel
     * 
     * I think this is dangerous..
     */
    public abstract async deleteChannel():Promise<boolean>
    /**
     * Create Channel with params (params are platform-specic.. so sad)
     * @param param Parameter
     */
    // public abstract createChannel(param?:unknown):Promise<SnowChannel>
    public abstract async decodeArgs(args:string[]):Promise<Array<string | SnowUser | SnowChannel>>
}
export type CreateChannelOpts = Partial<{
    category:string,
}>
export enum ConfigDepth {
    GUILD = 1,
    CHANNEL = 2,
}