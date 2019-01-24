import fs from "fs-extra"
import { DeepReadonly } from "minda-ts/build/main/types/deepreadonly"
import path from "path"
import { Connection, ConnectionOptions, createConnection,
    DeepPartial, EntitySchema, FindConditions, Repository } from "typeorm"
import BaseGuildCfg, { GidType } from "./baseguildcfg"
export const debugPath = path.resolve(__dirname, `../../../${(__dirname.indexOf("build") >= 0) ? "../" : ""}`)

export type SnowSchema<T> = Pick<T, Exclude<keyof T, keyof BaseGuildCfg>>
/**
 * Per Group / Channel Settings
 */
export default class SnowConfig<T extends BaseGuildCfg> {
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
            synchronize: true,
        })
        this.repo = this.connection.getRepository(this.schema)
    }
    public async getConfig(gid:string, prov:string) {
        const ids = {
            gid,
            provider: prov,
        }
        let element = await this.repo.findOne(ids as FindConditions<T>)
        if (element == null) {
            element = await this.repo.create(ids as DeepPartial<T>)
            await this.repo.save(element as any)
            element = await this.repo.findOne(ids as FindConditions<T>)
        }
        return element as SnowSchema<T>
    }
    public async getValue<K extends keyof SnowSchema<T>>
        (gid:GidType, prov:string, key:K):Promise<T[K]> {
        const schema = await this.getConfig(gid, prov)
        return schema[key]
    }
    public async setConfig(conf:SnowSchema<T>, gid?:GidType, prov?:string) {
        if (gid != null) {
            conf["gid"] = gid
        }
        if (prov != null) {
            conf["provider"] = prov
        }
        await this.repo.save(conf as any)
    }
    public async setValue<K extends keyof SnowSchema<T>>
        (gid:GidType, prov:string, key:K, value:T[K]) {
        const schema = await this.getConfig(gid, prov)
        schema[key] = value
        await this.repo.save(schema as any)
    }
}
export class SnowConfigSimple<T extends BaseGuildCfg> {
    private instance:SnowConfig<T>
    private gid:GidType
    private provider:string
    public constructor(ins:SnowConfig<T>, gid:GidType, prov:string) {
        this.instance = ins
        this.gid = gid
        this.provider = prov
    }
    /**
     * Get all of settings
     */
    public async getAll():Promise<Readonly<SnowSchema<T>>> {
        return this.instance.getConfig(this.gid, this.provider)
    }
    /**
     * Get value of config
     * @param key Field name
     */
    public async getValue<K extends keyof SnowSchema<T>>(key:K) {
        return this.instance.getValue(this.gid, this.provider, key)
    }
    /**
     * Set value of config
     * @param key Field name
     * @param value Data
     */
    public async setValue<K extends keyof SnowSchema<T>>(key:K, value:T[K]) {
        return this.instance.setValue(this.gid, this.provider, key, value)
    }
}