import { Column, Entity, PrimaryColumn, PrimaryGeneratedColumn } from "typeorm"

@Entity()
export default class PriGlobalStore {
    @PrimaryGeneratedColumn()
    public id:number
    public gid:number
    @PrimaryColumn()
    public key:string
    @Column()
    public value:string
}