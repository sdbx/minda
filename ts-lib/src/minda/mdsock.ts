import { EventEmitter } from "events"
import { Socket } from "net"
import { Command } from "./mdcommand"
import { Chated, Connected, Event } from "./mdevent"

export class MindaSocket extends EventEmitter {
    private sock = new Socket()
    private connected = false
    private buffer = ""
    constructor(ip:string, port:number, invite:string) {
        super()
        this.sock.on("data", this.onData.bind(this))
        this.sock.connect(port, ip, () => {
            this.connected = true
            this.send({
                type: "connect",
                invite
            })
        })
    }

    public send(cmd:Command) {
        if (this.connected) {
            this.sock.write(JSON.stringify(cmd))
        }
    }

    private onData(raw:Buffer) {
        const data = raw.toString("utf8")
        if (data.indexOf("\n") < 0) {
            this.buffer += data
        } else {
            const msg = this.buffer + data.substring(0, data.indexOf('\n'))
            this.buffer = data.substring(data.indexOf("\n") + 1)
            const event:Event = JSON.parse(msg)
            switch (event.type) {
                case "connected": {
                    this.emit("connected", event as Connected)
                    break
                }
                case "chated": {
                    this.emit("chated", event as Chated)
                    break
                }
            }
        }
    }
}