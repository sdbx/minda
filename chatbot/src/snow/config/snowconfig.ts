import path from "path"
import { Connection, ConnectionOptions, createConnection, Repository } from "typeorm"
import RawConfig from "./rawconfig"
const root = path.resolve(__dirname, "../../../")
const options:ConnectionOptions = {
    type: "sqlite",
    database: `${root}/config/config.sqlite`,
    entities: [RawConfig],
    logging: true,
}

export default class SnowConfig {
    protected connection:Connection
    protected rawRepo:Repository<RawConfig>
    public async init() {
        this.connection = await createConnection(options)
        this.rawRepo = this.connection.getRepository(RawConfig)
    }
}