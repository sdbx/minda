/**
 * Deep-Readonly type (recursive)
 * 
 * From: https://stackoverflow.com/questions/41879327/deepreadonly-object-typescript/49670389
 */
/* tslint:disable */
export type DeepReadonly<T> =
    T extends (infer R)[] ? DeepReadonlyArray<R> :
    T extends Function ? T :
    T extends object ? DeepReadonlyObject<T> :
    T;

interface DeepReadonlyArray<T> extends ReadonlyArray<DeepReadonly<T>> {}

type DeepReadonlyObject<T> = {
    readonly [P in keyof T]: DeepReadonly<T[P]>;
};

/**
 * Make deep-readonly object
 * 
 * From: https://stackoverflow.com/questions/41299642/how-to-use-javascript-proxy-for-nested-objects
 */
export function asReadonly<T extends object>(obj:T) {
    const validator:ProxyHandler<object> = {
        get: (target, key) => {
            const o = target[key]
            if (typeof o === "object" && o !== null) {
                return new Proxy(target[key], validator)
            } else {
                return target[key]
            }
        },
        set: (o, prop, name) => {
            throw new Error("ReadOnly Value in " + prop.toString())
        }
    }
    return new Proxy<T>(obj, validator) as DeepReadonly<T>
}