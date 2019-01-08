import { DeepReadonly } from "../types/deepreadonly"
import { Serializable } from "../types/serializable"
import { MindaClient } from "./mdclient"
import { MindaCredit } from "./mdcredit"
import { extractContent, reqGet, reqPost } from "./mdrequest"
import { MSPerm } from "./structure/msperm"
import { MSUser } from "./structure/msuser"

export class MindaAdmin extends MindaClient {
    public users:DeepReadonly<MSUser[]> = []
    public constructor(token:string | MindaCredit) {
        super(token)
    }
    /**
     * 유저 목록을 불러옵니다.
     */
    public async listUser() {
        const res = await extractContent<MSUser[]>(reqGet("GET", "/admin/users/", this.token))
        res.sort((a, b) => a.id - b.id)
        this.users = res
        console.log(JSON.stringify(res, null, 4))
        return res
    }
    /**
     * 유저를 생성합니다.
     * @param user 맨들 유저
     */
    public async createUser(user:MakeUser, uid?:number) {
        const users = await this.listUser()
        let id:number = users.length + 1
        if (uid != null) {
            id = uid
        } else {
            for (let i = 1; i <= users.length; i += 1) {
                if (users[i - 1].id !== i) {
                    id = i
                    break
                }
            }
        }
        const sID = uid != null
        const res = await reqPost(sID ? "PUT" : "POST", `/admin/users/${sID ? id + "/" : ""}`, this.token, {
            id,
            picture: null,
            ...user,
            permission: {
                ...user,
            }
        })
        if (res.ok) {
            await this.listUser()
        }
    }
    /**
     * 유저를 삭제합니다. (닉네임을 가진 **모든** 유저)
     * @param username 유저 이름
     */
    public async deleteUsersByName(username:string) {
        const users = (await this.listUser()).filter((v) => v.username === username)
        for (const user of users) {
            await this.deleteUserById(user.id, false)
        }
        await this.listUser()
    }
    /**
     * 유저를 삭제합니다.
     * @param id 유저 ID
     */
    public async deleteUserById(id:number, sync = true) {
        const res = await reqGet("DELETE", `/admin/users/${id}/`, this.token)
        if (res.ok) {
            if (sync) {
                await this.listUser()
            }
        } else {
            throw new Error("Operation Rejected.")
        }
    }
    public async getTokenOfUser(id:number) {
        
    }
}
type Omit<T, K extends keyof T> = Pick<T, Exclude<keyof T, K>>
type MakeUser = Omit<MSUser, "id" | "permission"> & MSPerm
type PrimitiveKeys<T> = { [K in keyof T]-?: T[K] extends string | number | boolean ? K : never }[keyof T]
type SearchRect = MSUser & MSPerm

// type NonFunctionPropertyNames<T> = { [K in keyof T]: T[K] extends Function ? never : K }[keyof T];
