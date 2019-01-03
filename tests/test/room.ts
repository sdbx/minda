import "mocha"
import { MindaClient } from "minda-ts"

const roomConf = {
    name: "hello",
    black: -1,
    white: -1,
    king: -1,
    rule: ""
}

describe("A user in the room", () => {
    it("should be the king in anohter room", (done) => {
        (async () => {
            let client = new MindaClient("black")
            let room = await client.createRoom(roomConf)
            room.onClose.sub(() => {
                setTimeout(done, 500)
            })
            let room2 = await client.createRoom(roomConf)
            room2.close()
        })()
    })

    it("should be able to join the room created", (done) => {
        (async () => {
            let client = new MindaClient("black")
            let room = await client.createRoom(roomConf)
            let client2 = new MindaClient("white")
            let room2 = await client2.joinRoom(room.id)
            room.close()
            room2.close()
            setTimeout(done, 500)
        })()
    })
    it("should be able to chat", (done) => {
        (async () => {
            let client = new MindaClient("black")
            let room = await client.createRoom(roomConf)
            let client2 = new MindaClient("white")
            let room2 = await client2.joinRoom(room.id)
            room.onChat.sub((chat) => {
                room.close()
                room2.close()
                if (chat.content == "Hello") {
                    setTimeout(done, 500)
                } else {
                    done(new Error("recieved different content"))
                }
            })
            room2.sendChat("Hello")
        })()
    })
})