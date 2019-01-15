import { Column, Entity, PrimaryGeneratedColumn } from "typeorm"

@Entity()
export default class RawConfig {
    @PrimaryGeneratedColumn()
    public id:number
    @Column()
    public platform:string
    @Column()
    public subname:string
    @Column()
    public key:string
    @Column()
    public valuepack:string
}