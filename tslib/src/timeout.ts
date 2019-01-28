import { EventDispatcher, SignalDispatcher, SimpleEventDispatcher } from "strongly-typed-events"
import { TimerID, WebpackTimer } from "./webpacktimer"

type STE = EventDispatcher<any, any> | SimpleEventDispatcher<any> | SignalDispatcher
/**
 * 이벤트를 구독하여 dispatch 될 때까지 기다립니다.
 * @param event 이벤트
 * @param timeout 타임아웃 시간
 * @param executor 이벤트 발생시 실행할 함수
 */
export default function awaitEvent<E extends STE, R>(
    event:E, timeout:number,
    executor:(...args:EventParam<E>) => R | Promise<R>,
    nullAsContinue = false):Promise<R> {
    return new Promise<R>((res, rej) => {
        let timer:TimerID
        const fn = event.sub(async (...args:Array<unknown>) => {
            WebpackTimer.clearTimeout(timer)
            const r = await executor(...args as EventParam<E>)
            if (!nullAsContinue || r !== null) {
                fn()
                res(r)
            } else {
                timer = WebpackTimer.setTimeout(() => {
                    fn()
                    rej(new Error("TIMEOUT"))
                }, timeout)
            }
        })
        timer = WebpackTimer.setTimeout(() => {
            fn()
            rej(new Error("TIMEOUT"))
        }, timeout)
    })
}
type EventParam<T extends STE> =
    T extends EventDispatcher<infer P, infer R> ? [P, R] :
    T extends SimpleEventDispatcher<infer S> ? [S] :
    T extends SignalDispatcher ? [] :
    never
/*
                let timer:TimerID
                const fn = mindaRoom.onConnect.one(() => {
                    WebpackTimer.clearTimeout(timer)
                    this.connectedRooms.set(mindaRoom.id, mindaRoom)
                    res(mindaRoom)
                })
                timer = WebpackTimer.setTimeout(() => {
                    mindaRoom.onConnect.unsub(fn)
                    rej(new Error("TIMEOUT"))
                }, 5000)
 */