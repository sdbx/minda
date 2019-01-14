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
    await new Promise((res, rej) => {
        aClient.onReady.one(() => res())
    })
    const u = await aClient.createUser({
        username: "끼로",
        admin: false,
        picture: "kkiro.png",
    })
    console.log(u)
    console.log(aClient.me)
    console.log(await aClient.getTokenOfUser(u))
    console.log(JSON.stringify(await aClient.listGameServers(), null, 4))
    await aClient.removeUser(u)
}

run()