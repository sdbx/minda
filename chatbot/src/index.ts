import Ping from "./chatbot/ping"
import SnowCommand from "./snow/bot/snowcommand"
import SnowConfig from "./snow/config/snowconfig"
import DiscordSnow from "./snow/provider/discordsnow"

async function run() {
    const tokens = await SnowConfig.getTokens()
    if (tokens["discord"] != null) {
        const t = tokens["discord"]
        const snowD = new DiscordSnow(t)
        await snowD.init()
        snowD.addCommand(
            new SnowCommand("ping", async (context, arg1, arg2) => {
                const {channel, message} = context
                await channel.send("쪼리핑!")
            }, "string", "number")
                .withRequires(2)
                .withHelp("핑핑 쪼리핑을 날립니다", "시간", "날짜")
        )
        snowD.addCommand(
            new SnowCommand("pong", async (context) => {
                console.log("Pong")
            }).withHelp("퐁퐁")
        )
    }
}
run()