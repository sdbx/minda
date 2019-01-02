import "mocha"
import { MindaClient } from "minda-ts"

const roomConf = {
    name: "hello",
    black: -1,
    white: -1,
    king: -1,
    rule: ""
}

describe("Room", () => {
    it("should not allow a user to own more than one rooms", (done) => {
        (async () => {
            let client = new MindaClient("black")
            let room = await client.createRoom(roomConf)
            room.onClose.sub(() => {
                done()
            })
            let room2 = await client.createRoom(roomConf)
            room2.close()
        })()
    })

    it("should be able to join the room created", async () => {
        let client = new MindaClient("black")
        let room = await client.createRoom(roomConf)
        let client2 = new MindaClient("white")
        let room2 = await client2.joinRoom(room.id)
        room.close()
        room2.close()
    })
    it("should be able to chat", (done) => {
        (async () => {
            let client = new MindaClient("black")
            let room = await client.createRoom(roomConf)
            let client2 = new MindaClient("white")
            let room2 = await client2.joinRoom(room.id)
            room.onChat.sub((chat) => {
                room.close()
                if (chat.content == "Hello") {
                    done()
                } else {
                    done(new Error("recieved different content"))
                }
            })
            room2.sendChat("Hello")
            room2.close()
        })()
    })
})