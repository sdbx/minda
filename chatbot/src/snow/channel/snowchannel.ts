import { GidType } from "../config/baseguildcfg"
import SnowMessage from "../snowmessage"
import { SnowPerm } from "../snowperm"
import SnowUser from "../snowuser"

export default abstract class SnowChannel {
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
    public abstract async user(id:string):Promise<SnowUser>
    /**
     * Get all users from this channel
     */
    public abstract async userList():Promise<SnowUser[]>
    /**
     * Get Permisson from this channel for me
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
    public abstract async decodeArgs(args:string[]):Promise<Array<string | SnowUser | SnowChannel>>
}
export enum ConfigDepth {
    GUILD = 1,
    CHANNEL = 2,
}