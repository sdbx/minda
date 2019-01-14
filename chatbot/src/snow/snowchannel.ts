import SnowMessage from "./snowmessage"

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
    public abstract async getConfig(depth:ConfigDepth, key:string):Promise<unknown>
}
export enum ConfigDepth {
    GUILD = 1,
    CHANNEL = 2,
}