import { MindaClient } from "./minda/mdclient"

function sleep(ms) {
    return new Promise(resolve => {
        setTimeout(resolve,ms)
    })
}

async function run() {
    const black = new MindaClient("black")
    const room = await black.createRoom({
        name: "hello",
        white: -1,
        black: -1,
        king: -1,
        rule: null
    })
    room.onChat.sub((v) => console.log(v))
    await sleep(1000)

    const white = new MindaClient("white")
    const room2 = await white.joinRoom(room.id)
    room2.onChat.sub((v) => console.log(v))
    room2.sendChat("안뇽")
    room.sendChat("그래")
}
run()