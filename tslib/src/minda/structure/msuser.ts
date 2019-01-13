import { MSPerm } from "./msperm"

export interface MSUser {
    id:number,
    username:string,
    // nullable
    picture?:string,
    permission:MSPerm,
}