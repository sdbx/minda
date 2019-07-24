import { MSInventory } from "./msinventory"
import { MSPerm } from "./msperm"

export interface MSUser {
    id:number,
    username:string,
    // nullable
    picture?:string,
    permission:MSPerm,
    inventory:MSInventory,
}
export interface MSUPicture {
    picture:string,
    picture_image:Buffer,
}