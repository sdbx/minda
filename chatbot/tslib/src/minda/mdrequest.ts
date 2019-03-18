import FormData from "form-data"
import mimeTypes from "mime-types"
import fetch, { Response } from "node-fetch"
import querystring from "querystring"
import request from "request-promise-native"
import { Serializable, SerializeObject } from "../types/serializable"
import { mdserver, mdversion } from "./mdconst"
import { MindaError } from "./mderror"
type GetType = "GET" | "DELETE"
type PostType = "POST" | "PUT"
/**
 * GET 타입 요청을 보냅니다.
 * @param suffix minda 서버의 REST 주소
 * @param token 인증키
 * @param param 파라메터
 * @returns JSON 혹은 Error
 */
export async function reqGet(type:GetType, suffix:string, token?:string, param:{[key in string]:string} = {}) {
    return req(type, false, suffix, token, param)
}
/**
 * POST 타입 요청을 보냅니다.
 * @param suffix minda 서버의 REST 주소
 * @param token 인증키
 * @param postParam POST로 보낼 `body`
 * @param urlParam URL 파라매터
 * @returns JSON 혹은 Error
 */
export async function reqPost(type:PostType, suffix:string, token?:string,
    postParam:SerializeObject = {}, urlParam:{[key in string]:string} = {}) {
    return req(type, true, suffix, token, urlParam, postParam)
}
/**
 * 바이너리 수신 요청을 보냅니다.
 */
export async function reqBinaryGet(type:"GET", suffix:string, token?:string) {
    const h = genHeader(suffix, token)
    suffix = h.suffix
    const response = await fetch(suffix, {
        method: type,
        headers: {
            ...h.headers,
            "Content-Type": "application/octet-stream",
        },
    })
    return response
}
export async function reqBinaryPost(type:"POST", suffix:string,
    formData:{ [K in string]: string | {filename:string, buf:Buffer} }, token?:string) {
    const h = genHeader(suffix, token)
    const form = new FormData()
    for (const [k, v] of Object.entries(formData)) {
        if (typeof v === "object") {
            form.append(k, v.buf, {
                filename: v.filename,
                contentType: mimeTypes.contentType(v.filename) || undefined,
            })
        } else {
            form.append(k, v)
        }
    }
    const response = await fetch(h.suffix, {
        method: type,
        body: form,
        headers: {
            ...h.headers,
            ...form.getHeaders(),
        },
    })
    return response
}
/**
 * content 출력
 * @param r Response
 */
export async function extractContent<T>(r:Promise<Response> | Response) {
    const rp = await r
    if (!rp.ok) {
        throw new MindaError(rp)
    }
    return await rp.json() as T
}
/**
 * 내부적으로 request를 보냅니다.
 * @param type HTTP Request 타입
 * @param isPost POST 형식인지?
 * @param suffix REST URL
 * @param token 토-큰
 * @param getParam URL에 붙일 파라매터
 * @param postParam POST 형식으로 보낼 파라매터
 */
async function req(type:GetType | PostType, isPost:boolean, suffix:string, token?:string,
    getParam:{[key in string]:string} = {}, postParam:SerializeObject = null) {
    const h = genHeader(suffix, token)
    suffix = h.suffix
    const qs = querystring.stringify(getParam, "&", "="/* ,{encodeURIComponent: (v:string) => }*/)
    // const post = querystring.stringify(postParam, "&", "=")
    const url = `${suffix}${qs.length >= 1 ? ("?" + qs) : ""}`
    const response = await fetch(url, {
        method: type,
        body: isPost ? (postParam == null ? undefined : JSON.stringify(postParam, null, 2)) : undefined,
        headers: {
            ...h.headers,
            "Content-Type": "application/json",
        },
    })
    return response
}
function genHeader(suffix:string, token:string) {
    if (!suffix.startsWith("http")) {
        if (!suffix.startsWith("/")) {
            suffix = "/" + suffix
        }
        suffix = `${mdserver}${suffix}`
    }
    let headers:{ [key in string]: string }
    if (!suffix.startsWith(mdserver)) {
        // other site request
        headers = {}
    } else {
        // minda site request
        headers = {
            "Authorization": token,
            "User-Agent": `minda-ts@${mdversion}`,
        }
    }
    return {
        suffix,
        headers,
    }
}