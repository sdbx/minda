// tslint:disable-next-line
type FunctionPropertyNames<T> = { [K in keyof T]: T[K] extends Function ? K : never }[keyof T];

export function bindFn<O extends object, T extends FunctionPropertyNames<O>>(obj:O, key:T):O[T] {
    const fn = obj[key] as unknown as (...args:any[]) => any
    return fn.bind(obj) as O[T]
}