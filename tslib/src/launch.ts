import { MindaAdmin } from "./minda/mdadmin"
import { MindaClient } from "./minda/mdclient"

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
    const aClient = new MindaAdmin("black")
    await aClient.listUser()
    /*
    await aClient.addUser({
        username: "끼로",
        admin: false,
        picture: "kkiro.png",
    })*/
    const findU = (await aClient.listUser()).find((v) => v.permission.admin)
    console.log(JSON.stringify(findU, null, 2))
    await aClient.deleteUsersByName("끼로")
    await aClient.listUser()
}

run()