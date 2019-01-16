import { EventDispatcher, SignalDispatcher } from "strongly-typed-events"
import { SnowPlugin } from "../bot/snowplugin"
import SnowChannel from "../snowchannel"
import SnowMessage from "../snowmessage"

export abstract class SnowProvider {
    public readonly onMessage = new EventDispatcher<SnowChannel, SnowMessage>() 
    public readonly onReady:SignalDispatcher = new SignalDispatcher()
    protected token:string
    protected plugins:SnowPlugin[] = []
    public abstract init():Promise<void>
    public addPlugin(plugin:SnowPlugin) {
        this.plugins.push(plugin)
        plugin.bindPlugin(this)
    }
}
