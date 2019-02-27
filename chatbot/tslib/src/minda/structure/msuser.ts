import { MSInventory } from "./msinventory"
import { MSPerm } from "./msperm"

export interface MSUser {
    id:number,
    username:string,
    // nullable
    picture?:number,
    permission:MSPerm,
    inventory:MSInventory,
}