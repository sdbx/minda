import { cLog } from "chocolog"
import GlobalConfig from "./chatbot/globalcfg"
import BotConfig from "./chatbot/guildcfg"
import MindaExec from "./chatbot/mindaexec"
import JsonConfig from "./snow/config/jsonconfig"
import Snow from "./snow/snow"
import path from "path"
import process from "process"

(async () => {
    const debugPath = path.resolve(process.cwd(), process.argv.find((v) => v === "--vscode") != null ? "./" : "")
    const tokenStore = new JsonConfig(GlobalConfig, `${debugPath}/config/token.json5`).ro
    const snow = new Snow(tokenStore, `${debugPath}/config`, BotConfig)
    const authF = new MindaExec(tokenStore.minda, `${debugPath}/config`)
    if (!await authF.init()) {
        await authF.genToken()
    }
    await cLog.v(await snow.login())
    snow.addCommands(authF.commands)
})()