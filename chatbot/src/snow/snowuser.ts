import { Column, Entity, PrimaryGeneratedColumn } from "typeorm"

@Entity()
export default class SnowUser {
    /**
     * Unique ID of chat provider
     */
    @Column()
    public id:string
    /**
     * Chat platform of user
     */
    @Column()
    public platform:string
    /**
     * Display Nickname
     */
    @Column()
    public nickname:string
    /**
     * Profile of user
     */
    @Column()
    public profileImage:string
    public getUID() {
        return `${this.platform}#${this.id}`
    }
}