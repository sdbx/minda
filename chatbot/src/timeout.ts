import { ISubscribable } from "strongly-typed-events"
import { TimerID, WebpackTimer } from "./webpacktimer"

/**
 * 이벤트를 Promise형태로 구독합니다.
 * @param event 이벤트
 * @param timeout 제한시간
 */
export function subscribe<TArgs>(event: ISubscribable<(args: TArgs) => void>, timeout: number): Promise<TArgs> {
    const newTimer = (rej) => (
        WebpackTimer.setTimeout(() => {
            rej(new Error("TIMEOUT"))
        }, timeout)
    )
    let unsubscribe
    let timer
    return new Promise<TArgs>((res, rej) => {
        timer = newTimer(rej)
        unsubscribe = event.sub((args) => {
            res(args)
            WebpackTimer.clearTimeout(timer)
            timer = newTimer(rej)
        })
    }).catch((error) => {
        unsubscribe()
        throw error
    })
}