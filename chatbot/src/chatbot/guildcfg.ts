import { Column, Entity } from "typeorm"
import BaseGuildCfg from "../snow/config/baseguildcfg"

@Entity()
export default class BotConfig extends BaseGuildCfg {
    @Column("boolean", {
        default: false,
    })
    public babu:boolean
    @Column({
        default: "dontlike",
    })
    public jjoa:string
    @Column("bigint", {
        default: 53,
    })
    public kkiro:number
    @Column({
        default: "death",
    })
    public deasu:string
    @Column({
        default: "IhateKkiro",
    })
    public iLikeKkiro:string
}