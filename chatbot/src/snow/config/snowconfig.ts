import fs from "fs-extra"
import hash from "hash.js"
import { DeepReadonly } from "minda-ts/build/main/types/deepreadonly"
import path from "path"
import { Connection, ConnectionOptions, createConnection,
    DeepPartial, EntitySchema, FindConditions, Repository } from "typeorm"
import BaseGuildCfg, { GidType } from "./baseguildcfg"
import SimpleConfig from "./simpleconfig"
export const debugPath = path.resolve(
    process.cwd(), process.argv.find((v) => v === "--vscode") != null ? "../" : "")

export type SnowSchema<T> = Pick<T, Exclude<keyof T, keyof BaseGuildCfg>>
/**
 * Per Group / Channel Settings
 */
export default class SnowConfig<T extends BaseGuildCfg> extends SimpleConfig<T, BaseGuildCfg> {
    public constructor(schema:new () => T, storePath:string) {
        super(schema, storePath)
    }
    public async getConfig(gid:string, prov:string) {
        const ids = {
            gid,
            provider: prov,
        }
        return this.getAll(ids)
    }
    public async getValue<K extends keyof SnowSchema<T>>
        (gid:GidType, prov:string, key:K):Promise<T[K]> {
        const schema = await this.getConfig(gid, prov)
        return schema[key]
    }
    public async setConfig(conf:SnowSchema<T>, gid?:GidType, prov?:string) {
        return this.setAll(conf, {
            gid,
            provider: prov,
        })
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