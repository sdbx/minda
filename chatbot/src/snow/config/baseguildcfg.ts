import { Entity, PrimaryColumn, PrimaryGeneratedColumn, Column } from "typeorm"

export type GidType = string
@Entity()
export default class BaseGuildCfg {
    @PrimaryColumn()
    public gid:GidType
    @PrimaryColumn()
    public provider:string
}