import { MindaCredit } from "./minda/mdcredit"
import { requestPost } from "./minda/req"


async function run() {
    const mindaC = new MindaCredit()
    await mindaC.listProvider()
}
run()