/* tslint:disable */
/**
 * Serializable type defintion.
 */
export type Serializable = string | number | boolean | SerializeObject | SerializeArray
export interface SerializeObject {
    [x:string]: Serializable;
}
export interface SerializeArray extends Array<Serializable> { }
/**
 * Serializify type
 */
// support type
type Diff<T, U> = T extends U ? never : T;  // Remove types from T that are assignable to U
type Filter<T, U> = T extends U ? T : never;  // Remove types from T that are not assignable to U
type Omit<T, K extends keyof T> = Pick<T, Exclude<keyof T, K>>
type FunctionPropertyNames<T> = { [K in keyof T]: T[K] extends Function ? K : never }[keyof T];
type FunctionProperties<T> = Pick<T, FunctionPropertyNames<T>>;
type NonFunctionPropertyNames<T> = { [K in keyof T]: T[K] extends Function ? never : K }[keyof T];
type NonFunctionProperties<T> = Pick<T, NonFunctionPropertyNames<T>>;
type SerializablePropertyNames<T> = { [K in keyof T]: T[K] extends Function ? never : (T[K] extends Serializable ? K : never) }[keyof T];
export type SerializableProperties<T> = Pick<T, SerializablePropertyNames<T>>;
// serializify
/**
 * Remove all non-serialize properties
 * 
 * Recursive
 */
export type Serializify<T> =
    T extends (infer R)[] ? SerialArray<R> :
    T extends Function ? never :
    T extends object ? SerialObject<T> :
    T extends Serializable ? T :
    never;
type SerialObject<T> = SerializableProperties<{
    [P in keyof T]: Serializify<T[P]>
}>
interface SerialArray<T> extends Array<Serializify<T>> {}
// functional
export type Functional<T> = FunctionProperties<T>
/*
export type DeepReadonly<T> =
    T extends (infer R)[] ? DeepReadonlyArray<R> :
    T extends Function ? T :
    T extends object ? DeepReadonlyObject<T> :
    T;

interface DeepReadonlyArray<T> extends ReadonlyArray<DeepReadonly<T>> {}

type DeepReadonlyObject<T> = {
    readonly [P in keyof T]: DeepReadonly<T[P]>;
};
*/