import { Entity, PrimaryColumn, PrimaryGeneratedColumn, Column } from "typeorm"

@Entity()
export default class BaseGuildCfg {
    @PrimaryGeneratedColumn()
    public id:number
    @PrimaryColumn()
    public gid:number
    @PrimaryColumn()
    public provider:string
}