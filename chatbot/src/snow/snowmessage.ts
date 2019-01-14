import SnowUser from "./snowuser"

export default class SnowMessage {
    /**
     * The id of message (mostly this is number. But sometimes bigint needs...
     * So, let's use BigInt with `ESNEXT`)
     */
    public id:bigint
    /**
     * The author of This message. 
     */
    public author:SnowUser
    /**
     * Text Content of message (if null, it remains to "")
     */
    public content:string
    /**
     * Image of message (can be null)
     */
    public image:string
    /**
     * Other additional info Fields
     * 
     * Ex. Attachment, URL, etc.
     */
    public fields:MessageField[]
}
export interface MessageField {
    type:"field" | "file" | "image",
    data:string,
}