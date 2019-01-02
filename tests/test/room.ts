import "mocha"
import { MindaClient } from "minda-ts"

describe("Create room", () => {
    it("should not allow a user to own more than one rooms", (done) => {
        (async () => {
            let client = new MindaClient("black")
            let room = await client.createRoom({
                name: "hello",
                black: -1,
                white: -1,
                king: -1,
                rule: ""
            })
            room.onClose.sub(() => {
                done()
            })
            let room2 = await client.createRoom({
                name: "hello",
                black: -1,
                white: -1,
                king: -1,
                rule: ""
            })
            room2.close()
        })()
    })

    it("should be able to join the room created", (done) => {
        (async ()=> {
            let client = new MindaClient("black")
            let room = await client.createRoom({
                name: "hello",
                black: -1,
                white: -1,
                king: -1,
                rule: ""
            })
            room.onEnter.sub(() => {
                done()
            })
            let client2 = new MindaClient("white")
            let room2 = await client2.joinRoom(room.id)
            room.close()
            room2.close()
        })()
    })
})