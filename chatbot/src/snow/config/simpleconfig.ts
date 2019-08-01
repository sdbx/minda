import { cLog } from "chocolog"
import fs from "fs-extra"
import hash from "hash.js"
import "sqlite3"
import path from "path"
import {
    Connection, ConnectionOptions, createConnection,
    DeepPartial, EntitySchema, FindConditions, Repository
} from "typeorm"
import { Serializify } from "../../types/serializable"
import BaseGuildCfg, { GidType } from "./baseguildcfg"

type ExcludeObject<T, B> = Pick<T, Exclude<keyof T, keyof B>>
export default class SimpleConfig<T extends B, B extends object> {
    protected schema:new () => T
    protected repo:Repository<T>
    protected dbpath:string
    protected connection:Connection
    public constructor(schema:new () => T, storePath:string) {
        this.schema = schema
        this.dbpath = path.resolve(storePath)
    }
    public async connect() {
        const name = `${this.schema.name}_${
            Buffer.from(hash.sha1().update(this.dbpath).digest("hex"), "hex").toString("base64")}`
        cLog.d(name)
        this.connection = await createConnection({
            type: "sqlite",
            database: this.dbpath,
            entities: [this.schema],
            logging: true,
            synchronize: true,
            name,
        })
        this.repo = this.connection.getRepository(this.schema)
    }
    public async getAll(query:Serializify<B>) {
        let element = await this.repo.findOne(query as FindConditions<T>)
        if (element == null) {
            element = await this.repo.create(query as DeepPartial<T>)
            await this.repo.save(element as any)
            element = await this.repo.findOne(query as FindConditions<T>)
        }
        return element as ExcludeObject<T, B>
    }
    public async get<K extends keyof ExcludeObject<T, B>>
        (query:Serializify<B>, key:K):Promise<T[K]> {
        const schema = await this.getAll(query)
        return schema[key]
    }
    public async setAll(conf:ExcludeObject<T, B> | T, query?:Serializify<B>) {
        if (query != null) {
            for (const [key, value] of Object.entries(query)) {
                if (key != null) {
                    conf[key] = value
                }
            }
        }
        await this.repo.save(conf as any)
    }
    public async set<K extends keyof ExcludeObject<T, B>>
        (query:Serializify<B>, key:K, value:T[K]) {
        const schema = await this.getAll(query)
        schema[key] = value
        await this.repo.save(schema as any)
    }
}