// tslint:disable-next-line
type FunctionPropertyNames<T> = { [K in keyof T]: T[K] extends Function ? K : never }[keyof T];

export function bindFn<O extends object, T extends Function>(obj:O, fn:T):T {
    return fn.bind(obj) as T
}