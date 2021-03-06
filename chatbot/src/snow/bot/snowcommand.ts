import debug from "debug"
import SnowChannel, { ConfigDepth } from "../channel/snowchannel"
import BaseGuildCfg from "../config/baseguildcfg"
import { SnowConfigSimple, SnowSchema } from "../config/snowconfig"
import { SnowProvider } from "../provider/snowprovider"
import SnowMessage from "../snowmessage"
import SnowUser from "../snowuser"

// const test:FilteredKeys<[1,"a",true],[1,"a"]> = []

export default class SnowCommand<
    C extends object, P extends AllowDecode[] = any[]> implements CommandConstructor<C, P> {
    /**
     * 명령어 이름
     */
    public readonly name:string
    /**
     * 필요 파라메터 수
     */
    public reqLength:number
    /**
     * 파라메터 이름
     */
    public paramNames:{[K in keyof P]:string}
    /**
     * 명령어 설명
     */
    public description:string
    /**
     * 파라메터 타입
     */
    public readonly paramTypes:P
    /**
     * 실행할 함수
     */
    public readonly func:(context:SnowContext<C>, ...args:ParamDecodeAll<P>) => unknown
    /**
     * Create command Receiver
     */
    public constructor(instance:CommandConstructor<C, P>, ...pType:P) {
        this.name = instance.name
        this.reqLength = instance.reqLength != null ? instance.reqLength : pType.length
        this.paramNames = instance.paramNames
        this.description = instance.description
        this.paramTypes = pType
        this.func = instance.func
    }
    /**
     * 필수 파라메터의 갯수를 정합니다.
     * @param length 갯수
     */
    public withRequires(length:number) {
        this.reqLength = length
        return this
    }
    /**
     * 도움말을 위해 설명을 추가합니다.
     * @param desc 전체적인 명령어 도움말
     * @param paramName 파라메터당 이름
     */
    public withHelp(desc:string, ...paramName:{[K in keyof P]:string}) {
        this.description = desc
        this.paramNames = paramName
        return this
    }
    /**
     * 파라메터의 형식이 올바른지 체크합니다.
     * @param params 
     */
    public checkParam(params:AvaiableParams[]) {
        if (params.length < this.reqLength) {
            return false
        }
        for (let i = 0; i < params.length; i += 1) {
            const param = params[i]
            const thisType = this.paramTypes[i]
            if (thisType === "string" && (typeof param === "number" || typeof param === "boolean")) {
                continue
            }
            if (typeof param === thisType) {
                continue
            }
            if (typeof param === "object") {
                if (param instanceof SnowUser && thisType === "SnowUser") {
                    continue
                }
            }
            return false
        }
        return true
    }
    /**
     * 기존 파라메터를 변환하여 타입에 맞게 출력합니다.
     * @param params 기존 파라메터
     */
    public convertParam(params:AvaiableParams[]) {
        const out:AllowEncode[] = []
        for (let i = 0; i < params.length; i += 1) {
            const param = params[i]
            const thisType = this.paramTypes[i]
            if (thisType === "string" && (typeof param === "number" || typeof param === "boolean")) {
                out.push(param.toString())
            } else if (typeof param === "object") {
                if (param instanceof SnowUser && thisType === "SnowUser") {
                    out.push(param)
                }
            } else if (typeof param === thisType) {
                out.push(param)
            }
        }
        return out
    }
    public async execute(context:SnowContext<C>,params:ParamDecodeAll<P>) {
        const result = await this.func(context, ...this.convertParam(params) as any)
        await context.updateGroupConfig()
        await context.updateChannelConfig()
        console.log("Executed Command.")
        return result
    }
    /*
    public bindProvider(prov:SnowProvider) {
        prov.onMessage.sub((ch, msg) => this.handleMessage(ch, msg))
        // prov.onReady.sub(() => this.handleReady())
        const test = new SnowCommand("Hello", "World!", (snow:SnowContext, str:string) => {
            return Promise.resolve()
        })
    }
    public abstract async handleMessage(channel:SnowChannel, message:SnowMessage):Promise<void>
    public async handleReady() {
        console.log("Ready!")
    }
    protected async validateCommand(cmds:string | string[], ch:SnowChannel, str:string | SnowMessage) {
        if (!Array.isArray(cmds)) {
            cmds = [cmds]
        }
        if (str instanceof SnowMessage) {
            str = str.content
        }
        const splitInput = str.split(/\s+/ig)
        for (const cmd of cmds) {
            if (str.startsWith(this.prefix + cmd)) {
                return {
                    command: cmd,
                    args: await ch.decodeArgs(splitInput.splice(0, 1)),
                }
                break
            }
        }
        return null
    }
    */
}
/**
 * Command constructor params
 */
export interface CommandConstructor<C extends object, P extends AllowDecode[] = any[]> {
    /**
     * Name of command
     */
    name:string;
    /**
     * Exec function
     */
    func:(context:SnowContext<C>, ...args:ParamDecodeAll<P>) => unknown;
    /**
     * Parameter names
     */
    paramNames:{[K in keyof P]:string};
    /**
     * Command Description
     */
    description:string;
    /**
     * Require Length of parameter
     */
    reqLength?:number;
}
export interface SnowContext<C extends object> {
    channel:SnowChannel,
    message:SnowMessage,
    configChannel:SnowSchema<C>,
    configGroup:SnowSchema<C>,
    updateChannelConfig:() => Promise<void>,
    updateGroupConfig:() => Promise<void>,
}

/**
 * Types
 */
export type AvaiableParams = string | number | boolean | SnowUser | SnowChannel
type ParamEncode<T extends AllowEncode> =
    T extends string ? "string" :
    T extends number ? "number" :
    T extends boolean ? "boolean" :
    T extends undefined ? never :
    // tslint:disable-next-line
    T extends Function ? never :
    T extends SnowUser ? "SnowUser" :
    T extends SnowChannel ? "SnowChannel" :
    never
export type ParamEncodeAll<T extends AllowEncode[]> = {
    [K in keyof T]: T[K] extends AllowEncode ? ParamEncode<T[K]> : never
}
export type ParamDecodeAll<T extends AllowDecode[]> = {
    [K in keyof T]: T[K] extends AllowDecode ? ParamDecode<T[K]> : never
}
type ParamDecode<T extends AllowDecode> =
    T extends "string" ? string :
    T extends "number" ? number :
    T extends "boolean" ? boolean :
    T extends "SnowUser" ? SnowUser :
    T extends "SnowChannel" ? SnowChannel :
    never
type AllowEncode = AvaiableParams
export type AllowDecode = ParamEncode<AllowEncode>