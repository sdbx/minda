import fs from "fs-extra"
import path from "path"
import { Connection, ConnectionOptions, createConnection, EntitySchema, Repository, FindConditions } from "typeorm"
import BaseGuildCfg from "./baseguildcfg"
export const debugPath = path.resolve(__dirname, `../../../${(__dirname.indexOf("build") >= 0) ? "../" : ""}`)

export default class SnowConfig<T extends BaseGuildCfg> {
    public static async getTokens():Promise<{[k in string]:string}> {
        try {
            const buf = await fs.readFile(`${debugPath}/config/token.json`)
            return JSON.parse(buf.toString("utf8"))
        } catch {
            await fs.writeFile(`${debugPath}/config/token.json`, JSON.stringify({
                example: "5353",
            }, null, 4))
            return {}
        }
    }
    protected schema:new () => T
    protected repo:Repository<T>
    protected dbpath:string
    protected connection:Connection
    public constructor(schema:new () => T, storePath:string) {
        this.schema = schema
        this.dbpath = path.resolve(storePath)
    }
    public async connect() {
        this.connection = await createConnection({
            type: "sqlite",
            database: this.dbpath,
            entities: [this.schema],
            logging: true,
        })
        this.repo = this.connection.getRepository(this.schema)
    }
    public async getConfig(gid: number, prov: string) {
        try {
            const uniqueSchema = await this.repo.findOne({
                gid,
                provider: prov,
            } as FindConditions<T>)
            return uniqueSchema
        } catch (err) {
            console.log(err)
            return this.repo.create()
        }
    }
    public async getValue<K extends Exclude<keyof T, keyof BaseGuildCfg>>
        (gid:number, prov:string, key:K):Promise<T[K]> {
        const schema = await this.getConfig(gid, prov)
        return schema[key]
    }
    public async setValue<K extends Exclude<keyof T, keyof BaseGuildCfg>>
        (gid:number, prov:string, key:K, value:T[K]):Promise<void> {
        const schema = await this.getConfig(gid, prov)
        schema[key] = value
        await this.repo.save(schema as any)
    }
}