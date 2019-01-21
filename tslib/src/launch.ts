import { MindaAdmin, MindaClient, MindaCredit, MindaRoom } from "./index"

async function run() {
    /*
    const client = new MindaClient("black")
    await client.createRoom({
        name: "hello",
        black:-1,
        white:-1,
        king:-1,
        rule: "",
    })
    */
    const aClient = new MindaAdmin("WU7htx_4_helo4FO3Im44pU=")
    await aClient.init()
    const black = new MindaClient("black")
    await black.init()
    const white = new MindaClient("white")
    await white.init()
    const room = await black.createRoom("맞짱1")
    white.joinRoom(room)
}

run()