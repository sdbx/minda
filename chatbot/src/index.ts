import Ping from "./chatbot/ping"
import SnowConfig from "./snow/config/snowconfig"
import DiscordSnow from "./snow/provider/discordsnow"

async function run() {
    const tokens = await SnowConfig.getTokens()
    if (tokens["discord"] != null) {
        const t = tokens["discord"]
        const snowD = new DiscordSnow(t)
        await snowD.init()
        snowD.addPlugin(new Ping("$"))
    }
}
run()