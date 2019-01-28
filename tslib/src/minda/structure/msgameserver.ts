import { MSRoom } from "./msroom"

export interface MSGameServer {
    name:string,
    addr:string,
    rooms:MSRoom[],
    last_ping:number,
}