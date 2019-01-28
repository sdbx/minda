import debug from "debug"

export function getFirst<T>(arr:T[] | T, df:T = null):T {
    if (arr != null && !Array.isArray(arr)) {
        return arr
    }
    if (arr == null || (arr as T[]).length === 0) {
        return df
    } else {
        return arr[0]
    }
}
export function logFn(key:string) {
    return (param:any, ...args:any[]) => debug(key)(param, args) 
}