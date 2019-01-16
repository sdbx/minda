import fs from "fs-extra"
import path from "path"
import { Connection, ConnectionOptions, createConnection, Repository } from "typeorm"
import RawConfig from "./rawconfig"
const root = path.resolve(__dirname, `../../../${(__dirname.indexOf("build") >= 0) ? "../" : ""}`)
const options:ConnectionOptions = {
    type: "sqlite",
    database: `${root}/config/config.sqlite`,
    entities: [RawConfig],
    logging: true,
}

export default class SnowConfig {
    public static async getTokens():Promise<{[k in string]:string}> {
        try {
            const buf = await fs.readFile(`${root}/config/token.json`)
            return JSON.parse(buf.toString("utf8"))
        } catch {
            await fs.writeFile(`${root}/config/token.json`, JSON.stringify({
                example: "5353",
            }, null, 4))
            return {}
        }
    }
    protected connection:Connection
    protected rawRepo:Repository<RawConfig>
    public async init() {
        this.connection = await createConnection(options)
        this.rawRepo = this.connection.getRepository(RawConfig)
    }
}