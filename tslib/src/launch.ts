import { MindaClient } from "./minda/mdclient"


(async ()=>{
    let client = new MindaClient("black")
    await client.createRoom({
    name: "hello",
    black:-1,
    white:-1,
    king:-1,
    rule: "",
})})()