import { Response } from "node-fetch"

export class MindaError extends Error {
    public errorCode:number
    public url:string
    public constructor(res:Response) {
        let str:string
        switch (res.status) {
            case 200:
            case 201: {
                // ?
                str = "비정상적인 코드 구현"
                break
            }
            case 403: {
                str = "인증되지 않음"
                break
            }
            case 404: {
                str = "존재하지 않음"
                break
            }
            case 500: {
                str = "Internal Server Error"
                break
            }
            default: {
                str = "Unknown: " + res.statusText
            }
        }
        super(str)
        this.name = "MindaAPI ERR"
        this.errorCode = res.status
        this.url = res.url
    }
}