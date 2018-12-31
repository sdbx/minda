import { MindaCredit } from "./minda/mdcredit"
import { requestPost } from "./minda/req"


async function run() {
    const mindaC = new MindaCredit()
    mindaC.onLogin.sub((token) => {
        console.log("Login success: " + token)
    })
    const providers = await mindaC.getProviders()
    const url = await mindaC.genOAuth(providers.pop())
    console.log(url)
    mindaC.watchLogin()
}
run()