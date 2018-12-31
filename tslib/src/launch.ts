import { MindaClient } from "./minda/mdclient"
import { MindaCredit } from "./minda/mdcredit"
import { requestPost } from "./minda/req"
import { WebpackTimer } from "./webpacktimer"


async function run() {
    const mindaC = new MindaCredit()
    /*
    mindaC.onLogin.sub((token) => {
        console.log("Login success: " + token)
        room(token)
    })
    const providers = await mindaC.getProviders()
    const url = await mindaC.genOAuth(providers.pop())
    console.log(url)
    mindaC.watchLogin()
    */
   room("black")
   WebpackTimer.setTimeout(() => room("white"), 1000)
}
async function room(token:string) {
    const mindaC = new MindaClient(token)
    const rooms = await mindaC.fetchRoom()
    const playRoom = await mindaC.connectRoom(mindaC.rooms[0])
    playRoom.onChat.sub((v) => console.log(v))
    playRoom.sendChat("안뇽")
}
run()