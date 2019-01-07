import { MindaClient } from "./minda/mdclient"

async function run() {
    const client = new MindaClient("black")
    await client.createRoom({
        name: "hello",
        black:-1,
        white:-1,
        king:-1,
        rule: "",
    })
}

run()