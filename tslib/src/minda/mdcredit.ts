import fetch from "node-fetch"
import { server } from "./mdconst"

export class MindaCredit {
    public async listProvider() {
        const id = await fetch(`${server}/auth/o/`, {method: "GET"})
            .then((res) => res.json() as unknown) as string[]
        console.log(id)
    }
}

enum KnownProvider {
    discord = "discord",

}