import fetch, { Response } from "node-fetch"
import querystring from "querystring"
import { Serializable, SerializeObject } from "../types/serializable"
import { mdserver, mdversion } from "./mdconst"
type GetType = "GET"
type PostType = "POST"
/**
 * GET 요청을 보냅니다.
 * @param suffix minda 서버의 REST 주소
 * @param token 인증키
 * @param param 파라메터
 * @returns JSON 혹은 Error
 */
export async function reqGet(suffix:string, token?:string, param:{[key in string]:string} = {}) {
    return req("GET", false, suffix, token, param)
}
/**
 * POST 요청을 보냅니다.
 * @param suffix minda 서버의 REST 주소
 * @param token 인증키
 * @param postParam POST로 보낼 `body`
 * @param urlParam URL 파라매터
 * @returns JSON 혹은 Error
 */
export async function reqPost(suffix:string, token?:string,
    postParam:SerializeObject = {}, urlParam:{[key in string]:string} = {}) {
        return req("POST", true, suffix, token, urlParam, postParam)
}
/**
 * content 출력
 * @param r Response
 */
export async function extractContent<T extends Serializable>(r:Promise<Response> | Response) {
    const rp = await r
    if (rp.status !== 200 && rp.status !== 201) {
        throw new Error(`${rp.url} / ${rp.status} : ${rp.statusText}`)
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
async function req(type:"GET" | "POST", isPost:boolean, suffix:string, token?:string,
    getParam:{[key in string]:string} = {}, postParam:SerializeObject = {}) {
    if (!suffix.startsWith("http")) {
        if (!suffix.startsWith("/")) {
            suffix = "/" + suffix
        }
        suffix = `${mdserver}${suffix}`
    }
    let headers:{[key in string]:string}
    if (!suffix.startsWith(mdserver)) {
        // other site request
        headers = {
            // not implement yet
            "Content-Type": isPost ? "x-www-form-urlencoded" : undefined,
        }
    } else {
        // minda site request
        headers = {
            "Authorization": token,
            "Content-Type": /* isPost ? "x-www-form-urlencoded" : */ "application/json",
            "User-Agent": `minda-ts@${mdversion}`,
        }
    }
    const qs = querystring.stringify(getParam, "&", "="/* ,{encodeURIComponent: (v:string) => }*/)
    // const post = querystring.stringify(postParam, "&", "=")
    const url = `${suffix}${qs.length >= 1 ? ("?" + qs) : ""}`
    const response = await fetch(url, {
        method: type,
        body: isPost ? JSON.stringify(postParam, null, 2) : undefined,
        headers,
    })
    return response
}