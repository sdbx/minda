import fetch from "node-fetch"
import { SignalDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import { TimerID, WebpackTimer } from "../webpacktimer"
import { mdserver } from "./mdconst"
import { extractContent, reqGet, reqPost } from "./mdrequest"
/**
 * 민다 서버 인증 모듈
 */
export class MindaCredit {
    /**
     * oAuth로 로그인을 성공했을때
     * 
     * @param 토큰
     */
    public readonly onLogin = new SimpleEventDispatcher<string>()
    /**
     * 기다려도 oAuth로 로그인을 안 했을때
     */
    public readonly onTimeout = new SignalDispatcher()
    protected providers:string[] = []
    protected reqid:string
    protected watchStamp:number
    protected watchTimer:TimerID
    /**
     * 토큰
     */
    protected _token:string
    // readonly
    /**
     * 토큰 (**읽기 전용**)
     */
    public get token() {
        return this._token
    }
    /**
     * oAuth 인증 대기시간
     */
    protected timeout:number
    public constructor(timeout = 600) {
        this.timeout = timeout
    }
    /**
     * 서버로부터 oAuth 제공자를 받습니다.
     * @returns [`discord`, `naver`] 이런것
     */
    public async getProviders() {
        try {
            return await extractContent<string[]>(reqGet("GET", "/auth/o/"))
        } catch (err) {
            /*
            console.error(err)
            return []
            */
            throw err
        }
    }
    /**
     * 로그인하기 위한 oAuth URL을 생성합니다.
     * @param provider oAuth 제공 URL (`getProviders()` 참조)
     */
    public async genOAuth(provider:string) {
        this.reqid = (await extractContent<{req_id:string}>(reqPost("POST", "/auth/reqs/"))).req_id
        return `${mdserver}/auth/o/${provider}/${this.reqid}/`
    }
    /**
     * 로그인이 됐는지 확인합니다. 로그인이 되면 token값이 부여됩니다.
     * @param fireEvent ~~빠이아이벤뜨~~ (기본: `false`)
     */
    public async logined(fireEvent = false) {
        if (this.reqid == null) {
            return false
        }
        const res = await reqGet("GET", `/auth/reqs/${this.reqid}/`)
        if (res.status === 403 || res.status === 400) {
            // not logined
            return false
        } else if (res.status === 200 || res.status === 201) {
            this._token = (await extractContent<{token:string}>(res)).token
            if (fireEvent) {
                this.onLogin.dispatch(this.token)
            }
            return true
        } else {
            throw new Error(`${res.url} / status ${res.status}`)
        }
    }
    /**
     * oAuth를 했는지 주기적으로 확인합니다.
     * 로그인시 `onLogin` 이벤트를 호출시킵니다.
     */
    public watchLogin() {
        if (this.watchTimer != null) {
            WebpackTimer.clearInterval(this.watchTimer)
        }
        if (this.reqid == null) {
            return
        }
        this.watchStamp = Date.now()
        this.watchTimer = WebpackTimer.setInterval(this.scheduleCheck.bind(this), 5 * 1000)
    }
    /**
     * 로그인 체크 - 타이머 돌리는 함수
     */
    protected async scheduleCheck() {
        // duplicate auth? false.
        if (this.reqid == null || this.token != null) {
            WebpackTimer.clearInterval(this.watchTimer)
            return
        }
        try {
            if (!await this.logined(true) && Date.now() - this.watchStamp >= this.timeout * 1000) {
                this.onTimeout.dispatch()
                WebpackTimer.clearInterval(this.watchTimer)
            }
        } catch (err) {
            console.error(err)
            // anyway.. try until timeout
        }
    }
}