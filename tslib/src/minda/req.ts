import req, { CookieJar } from "request"
import request from "request-promise-native"

export async function requestPost<T extends "utf8" | "euckr" | "binary">(
    method:"POST" | "PUT",
    url:QueryLike,
    body:{[key in string]:string | number | boolean} = {},
    encoding?:T,
    options:Partial<{
        referer:string,
        cookie:CookieJar,
    }> = {}):Promise<T extends "binary" ? Buffer : string> {
    const cookie = options.cookie !== undefined ? req.jar() : options.cookie
    if (encoding === "binary") {
        return Buffer.from("aa") as any
    } else {
        return "aa" as any
    }
}
type QueryLike = string | {url:string, qs:{[key in string]:string | number | boolean}}

export class MindaRequest {
    
}