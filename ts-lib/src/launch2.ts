import { MindaSocket } from "./minda"
import { Chated, Connected } from "./minda/mdevent"

const invite = "b0184b2e-ca30-4582-8e1d-d20d80e69cbb"
const sock = new MindaSocket("127.0.0.1", 5353, invite)
console.log("asdf")
sock.on("connected", (event:Connected) => {
    console.log(event)
    sock.send({
        type: "chat",
        content: "ㅎㅇ여"
    })
})
sock.on("chated", (event:Chated) => {
    console.log(event)
})