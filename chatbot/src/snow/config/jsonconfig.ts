import cloneDeep from "clone-deep"
import deepMerge from "deepmerge"
import fs from "fs-extra"
import json5 from "json5"
import { asReadonly } from "../../types/deepreadonly"
import { Serializable, Serializify } from "../../types/serializable"

export default class JsonConfig<T extends object> {
    protected path:string
    protected value:Serializify<T>
    /**
     * `path`에 파일을 만들면서 설정을 불러옵니다.
     * @param obj 기본값이 포함된 설정 구조
     * @param path 경로
     */
    public constructor(obj:new () => T, path:string) {
        this.value = cloneDeep(new obj())
        this.path = path
        this.load()
        this.save(this.value)
    }
    /**
     * 읽기 전용으로 불러옵니다
     */
    public get ro() {
        return asReadonly(this.value)
    }
    /**
     * 읽고 쓸 수 있는 데이터 형태로 불러옵니다.
     */
    public get rw() {
        const validator:ProxyHandler<object> = {
            get: (target, key) => {
                const o = target[key]
                if (typeof o === "object" && o !== null) {
                    return new Proxy(target[key], validator)
                } else {
                    return target[key]
                }
            },
            set: (o, prop, value) => {
                const back = o[prop]
                o[prop] = value
                try {
                    this.save(o)
                } catch (err) {
                    console.error(err)
                    o[prop] = back
                    return false
                }
                return true
            }
        }
        return new Proxy<Serializify<T>>(this.value, validator)
    }
    private save(obj:object) {
        fs.writeFileSync(this.path, json5.stringify(obj, null, 2))
    }
    private load() {
        if (fs.pathExistsSync(this.path)) {
            try {
                const json = json5.parse(fs.readFileSync(this.path, { encoding: "utf8" }))
                this.value = deepMerge(this.value, json)
            } catch (err) {
                console.error(err)
            }
        }
    }
}