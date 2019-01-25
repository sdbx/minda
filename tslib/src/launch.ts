import chalk from "chalk"
import enquirer, { prompt } from "enquirer"
import { MindaAdmin, MindaClient, MindaCredit, MindaRoom } from "./index"
import { reqPost } from "./minda/mdrequest"

async function login() {
    const credit = new MindaCredit(20000)
    const provs = await credit.getProviders()
    const selectProv = await prompt<{provider:string}>({
        type: "select",
        name: "provider",
        message: "oAuth 공급자를 선택해주세요.",
        choices: provs,
    })
    console.log("인증을 시작합니다. 아래 링크를 눌러주세요.")
    const oAuth = await credit.genOAuth(selectProv.provider)
    console.log(oAuth)
    credit.watchLogin()
}
async function run() {
    await login()
    return
    const aClient = new MindaAdmin("WU7htx_4_helo4FO3Im44pU=")
    await aClient.init()

    const mkToken = async (name:string) => 
        (await aClient.createUser(name, false)).token
    const black = new MindaClient(await mkToken("dBlack"))
    await black.init()
    const white = new MindaClient(await mkToken("dWhite"))
    await white.init()
    const blackG = await black.createRoom("맞짱1")
    const whiteG = await white.joinRoom(blackG)
    blackG.onChat.sub((str) => {
        console.log("채팅: " + str.content)
    })
    const bl:boolean[] = []
    bl.push(blackG.sendChat("Black Ready"))
    bl.push(whiteG.sendChat("White Ready"))
    bl.push(await blackG.setBlack(black.me))
    bl.push(await blackG.setWhite(white.me))
    bl.push(await blackG.startGame())
    bl.push(await whiteG.giveUp())
    console.log(bl)
}

run()