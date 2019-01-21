import { DeepReadonly } from "../types/deepreadonly"
import { Serializable, Serializify } from "../types/serializable"
import { MindaClient } from "./mdclient"
import { MindaCredit } from "./mdcredit"
import { MindaError } from "./mderror"
import { extractContent, reqGet, reqPost } from "./mdrequest"
import { MSGameServer } from "./structure/msgameserver"
import { MSPerm } from "./structure/msperm"
import { MSUser } from "./structure/msuser"
/**
 * 관리자 토큰을 이용하여 서버를 관리합니다.
 */
export class MindaAdmin extends MindaClient {
    /**
     * [어드민] 유저 목록
     */
    public users:DeepReadonly<MSUser[]> = []
    public constructor(token:string | MindaCredit) {
        super(token)
    }
    /**
     * [어드민] 유저 목록을 불러옵니다.
     */
    public async listUsers() {
        const res = await extractContent<MSUser[]>(reqGet("GET", "/admin/users/", this.token))
        res.sort((a, b) => a.id - b.id)
        this.users = [...res]
        return res
    }
    /**
     * [어드민] 현재 인식된 게임서버들의 리스트를 불러옵니다.
     */
    public async listGameServers() {
        const res = await extractContent<MSGameServer[]>(reqGet("GET", "/admin/gameservers/", this.token))
        return res
    }
    /**
     * [어드민] 유저를 생성합니다.
     * @param user 맨들 유저
     * @returns 맨들어진 유저
     */
    public async createUser(name:string, isAdmin:boolean, uid?:number) {
        const orgUsers = (await this.listUsers()).map((v) => v.id)
        const hasID = uid != null
        const res = await reqPost(hasID ? "PUT" : "POST",`/admin/users/${hasID ? uid + "/" : ""}`, this.token, {
            uid,
            picture: -1,
            username: name,
            permission: {
                admin: isAdmin,
            }
        } as Serializify<Omit<MSUser, "id">>)
        if (res.ok) {
            const users = await this.listUsers()
            return users.find((v) => orgUsers.findIndex((vi) => vi === v.id) < 0)
        } else {
            throw new MindaError(res)
        }
        return null
    }
    /**
     * [어드민] 유저를 제거합니다.
     * @param user 유저
     */
    public async removeUser(user:string | number | MSUser) {
        let users:number[]
        if (typeof user === "string") {
            users = this.users.filter((v) => v.username === user).map((v) => v.id)
        } else if (typeof user === "number") {
            users = [user]
        } else {
            users = [user.id]
        }
        return this.deleteUsersById(users)
    }
    /**
     * [어드민] 유저의 토큰을 가져옵니다.
     * @param id 유-저
     */
    public async getTokenOfUser(user:string | number | MSUser) {
        const res = await extractContent<string>(
            reqPost("POST", `/admin/users/${this.getUser(user)}/token/`, this.token))
        return res
    }
    /**
     * [내부] 다양한 타입으로부터 유저ID를 가져옵니다.
     * @param user 유-저
     */
    protected getUser(user:string | number | MSUser) {
        if (typeof user === "string") {
            return this.users.find((v) => v.username === user).id
        } else if (typeof user === "number") {
            return user
        } else {
            return user.id
        }
    }
    /**
     * [내부] 유저를 삭제합니다.
     * @param id 유저 ID
     */
    protected async deleteUsersById(ids:number[]) {
        for (const id of ids) {
            const res = await reqGet("DELETE", `/admin/users/${id}/`, this.token)
            if (!res.ok) {
                throw new MindaError(res)
            }
        }
        await this.listUsers()
    }
}
type Omit<T, K extends keyof T> = Pick<T, Exclude<keyof T, K>>
type MakeUser = Omit<MSUser, "id" | "permission"> & MSPerm
type PrimitiveKeys<T> = { [K in keyof T]-?: T[K] extends string | number | boolean ? K : never }[keyof T]
type SearchRect = MSUser & MSPerm

// type NonFunctionPropertyNames<T> = { [K in keyof T]: T[K] extends Function ? never : K }[keyof T];
