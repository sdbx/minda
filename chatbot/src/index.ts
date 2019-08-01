import GlobalConfig from "./chatbot/globalcfg"
import BotConfig from "./chatbot/guildcfg"
import MindaExec from "./chatbot/mindaexec"
import JsonConfig from "./snow/config/jsonconfig"
import Snow from "./snow/snow"

(async () => {
    const tokenStore = new JsonConfig(GlobalConfig, `./token.json5`).ro
    const snow = new Snow(tokenStore, `./config`, BotConfig)
    const authF = new MindaExec(tokenStore.minda, `./config`)
    if (!await authF.init()) {
        await authF.genToken()
    }
    await console.log(await snow.login())
    snow.addCommands(authF.commands)
})().catch((error) => {
    console.log(error)
})