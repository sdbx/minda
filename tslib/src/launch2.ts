import { MindaSocket } from "./minda"
import { Chated, Connected } from "./minda/mdevent"

const invite = "9006d221-409e-41ca-93c9-1c02e95adb99"
const invite2 = "ffe7811d-1a87-4dec-8e99-4dcf7e77e3cd"
const sock = new MindaSocket("127.0.0.1", 5353, invite)
const sock2 = new MindaSocket("127.0.0.1", 5353, invite2)
console.log("asdf")

sock.on("connected", (event:Connected) => {
    console.log("1:", event)
    sock.send({
        type: "chat",
        content: "ㅎㅇ여"
    })
})

sock2.on("connected", (event:Connected) => {
    console.log("2:", event)
})

sock.on("chated", (event:Chated) => {
    console.log("1:", event)
})

sock2.on("chated", (event:Chated) => {
    console.log("2:", event)
})
