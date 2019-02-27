import { createCanvas, Image, loadImage, registerFont } from "canvas"
import { cLog } from "chocolog"
import emojiUnicode from "emoji-unicode"
import fs from "fs-extra"
import sizeOf from "image-size"
import { MSGrid, MSUser, StoneType } from "minda-ts"
import fetch from "node-fetch"
import sharp from "sharp"
import { debugPath } from "../snow/config/snowconfig"
import awaitEvent from "../timeout"
import { blank1_1, blank1_2, blank1_3, blank1_4, blank1_6, blankChar, blankChar2 } from "./cbconst"

export async function renderBoard(board:MSGrid, pallate:Partial<{
    black:string,
    white:string,
    default:string,
}> = {}, sideinfo:Partial<{
    black:{username:string, stone?:number, image?:Buffer},
    white:{username:string, stone?:number, image?:Buffer},
    maxstone:number,
}> = {}) {
    // load font
    const ttf = `${debugPath}/res/NanumSquareRoundR.ttf`
    registerFont(ttf, { family: "NanumSquareRound" })
    // define size
    const hexagonSize = 1500
    const frameWidth = Math.floor(hexagonSize * 1.2211)
    const circleFR = Math.floor(frameWidth * 1.1)
    const canvas = createCanvas(
        circleFR,
        circleFR, "PDF")
    // load frame
    const frameBuffer = await fs.readFile(`${debugPath}/res/board.png`)
    const frameSizeI = sizeOf(frameBuffer)
    const scaledFrameH = Math.floor(frameSizeI.height / frameSizeI.width * frameWidth)
    const frame = await sharp(frameBuffer)
        .resize(frameWidth, scaledFrameH, { fit: "inside" })
        .toBuffer().then(loadImage)
    const ctx:CanvasRenderingContext2D = canvas.getContext("2d")
    // draw circle
    ctx.beginPath()
    const r = circleFR / 2
    ctx.arc(r, r, r, 0, 2 * Math.PI, false)
    ctx.fillStyle = "#f9edca"
    ctx.fill()
    // draw frame
    ctx.drawImage(frame, (circleFR - frameWidth) / 2, (circleFR - scaledFrameH) / 2)
    await drawHexagon(ctx, [Math.floor(circleFR / 2), Math.floor(circleFR / 2)], hexagonSize, "#2c2c2a", {
        board,
        blackImage: pallate.black,
        whiteImage: pallate.white,
        noStoneImage: pallate.default,
    })
    // draw userinfo
    const infoSize = Math.floor(hexagonSize / 8)
    const defaultPic = await fs.readFile(`${debugPath}/res/placeHolderProfileImage.png`)
    // copy
    if (sideinfo.black != null) {
        if (sideinfo.black.image == null) {
            sideinfo.black.image = defaultPic
        }
        await drawPicture(ctx, "left", {
            at: [(circleFR - infoSize * 3) / 2, 0],
            size: infoSize,
            backColor: "#222222",
            textColor: "#eeeeee",
            stone: sideinfo.black.stone,
            maxStone: sideinfo.maxstone,
        }, sideinfo.black.image, sideinfo.black.username)   
    }
    // paste
    if (sideinfo.white != null) {
        if (sideinfo.white.image == null) {
            sideinfo.white.image = defaultPic
        }
        await drawPicture(ctx, "right", {
            at: [(circleFR - infoSize * 3) / 2, circleFR - infoSize],
            size: infoSize,
            backColor: "#eeeeee",
            textColor: "#222222",
            stone: sideinfo.white.stone,
            maxStone: sideinfo.maxstone,
        }, sideinfo.white.image, sideinfo.white.username)
    }
    const buffer:Buffer = canvas.toBuffer()
    return buffer
}
/**
 * Draw hexagon
 * @param ctx Context
 * @param centerPoint Center point of hexagon 
 * @param hexaLength Hexagon's longest width
 * @param bgColor Background color
 * @param params Original parameters
 */
async function drawHexagon(ctx:CanvasRenderingContext2D,
    centerPoint:[number, number],
    hexaLength:number,
    bgColor:string,
    params:{
        board:MSGrid,
        blackImage:string,
        whiteImage:string,
        noStoneImage:string,
    }
    ) {
    // 0. config (0 < x <= 1/2)
    const elSize = 2 / 5
    const {board, blackImage, whiteImage, noStoneImage} = params
    /* first: parse color */
    const parseColor = (str:string, fault:string) => {
        if (str != null && str.startsWith("#") && /^#[0-9A-Fa-f]{6}$/ig.test(str)) {
            return str.toUpperCase()
        } else {
            return fault.toUpperCase()
        }
    }
    const colors = {
        black: parseColor(blackImage, "#eeeeee"),
        white: parseColor(whiteImage, "#111111"),
        default: parseColor(noStoneImage, "#ffefbc"), 
    }
    /* and draw background */
    if (bgColor != null) {
        ctx.fillStyle = bgColor
        ctx.beginPath()
        const [cx, cy] = centerPoint
        ctx.moveTo(Math.floor(cx - hexaLength / 2), cy)
        ctx.lineTo(Math.floor(cx - hexaLength / 4), Math.floor(cy - Math.sqrt(3) * hexaLength / 4))
        ctx.lineTo(Math.floor(cx + hexaLength / 4), Math.floor(cy - Math.sqrt(3) * hexaLength / 4))
        ctx.lineTo(Math.floor(cx + hexaLength / 2), cy)
        ctx.lineTo(Math.floor(cx + hexaLength / 4), Math.floor(cy + Math.sqrt(3) * hexaLength / 4))
        ctx.lineTo(Math.floor(cx - hexaLength / 4), Math.floor(cy + Math.sqrt(3) * hexaLength / 4))
        ctx.lineTo(Math.floor(cx - hexaLength / 2), cy)
        ctx.closePath()
        ctx.fill()
    }
    /* second: define constans */
    hexaLength = Math.floor(hexaLength * board.sqaureSize / (board.sqaureSize + Math.sqrt(2) - 1))
    // 1 * l / (2n)
    const oneX = hexaLength / (2 * board.sqaureSize)
    // 3^(1/2)l / (2n)
    const oneY = Math.sqrt(3) * hexaLength / (2 * board.sqaureSize)
    // stone/image width (not radios but width)
    const elementWidth = 2 * hexaLength * elSize / board.sqaureSize
    /* third: draw */
    // load image
    const getImage = async (url:string) => {
        if (url == null || !url.startsWith("http")) {
            return null
        }
        let binary = await fetch(url).then((v) => v.buffer())
        const roundedCorners = Buffer.from(
            `<svg><rect x="0" y="0" width="${elementWidth}" height="${elementWidth
            }" rx="${elementWidth}" ry="${elementWidth}"/></svg>`
            // `<svg><circle cx="${elementWidth / 2} cy="${elementWidth / 2}" r="${elementWidth / 2}"/></svg>`
        )
        binary = await sharp(binary)
            .resize(Math.ceil(elementWidth))
            .overlayWith(roundedCorners, { cutout: true })
            .toBuffer()
        return new Promise<Image>((res, rej) => {
            const img = new Image()
            img.onload = () => res(img)
            img.onerror = (err:any) => { res(null) }
            img.src = binary
        })
    }
    const images = {
        black: await getImage(blackImage),
        white: await getImage(whiteImage),
        default: await getImage(noStoneImage),
    }
    /**
     * Get XY, draw image or circle to position.
     */
    const grid = board.decodedGrid
    for (let row = 0; row < grid.length; row += 1) {
        for (let column = 0; column < grid[row].length; column += 1) {
            const stone = grid[row][column]
            const x = Math.floor(centerPoint[0] +
                (Math.abs(board.centerPosition - row) + 2 * (column - board.centerPosition)) * oneX)
            const y = Math.floor(centerPoint[1] + (row - board.centerPosition) * oneY)
            const drawCircle = (fillColor:string, r:number) => {
                ctx.beginPath()
                ctx.arc(x, y, r, 0, 2 * Math.PI, false)
                ctx.fillStyle = fillColor
                ctx.fill()
            }
            const drawImage = (fillImage:Image, cornerColor:string, r:number) => {
                ctx.drawImage(fillImage,
                    Math.floor(x - r),
                    Math.floor(y - r),
                    Math.round(r * 2), Math.round(r * 2))
                ctx.beginPath()
                ctx.arc(x, y, r, 0, 2 * Math.PI, false)
                ctx.strokeStyle = cornerColor
                ctx.lineWidth = Math.min(3, Math.floor(r / 20))
                ctx.stroke()
            }
            const draw = (fillImage:Image, fillColor:string, smallsize:boolean) => {
                let r = elementWidth / 2
                if (smallsize) {
                    r *= 0.7
                    r = Math.round(r)
                }
                if (fillImage != null) {
                    drawImage(fillImage, "#333333", r)
                } else {
                    drawCircle(fillColor, r)
                }
            }
            switch (stone) {
                case StoneType.black: {
                    draw(images.black, colors.black, false)
                } break
                case StoneType.white: {
                    draw(images.white, colors.white, false)
                } break
                case StoneType.void: {
                    draw(images.default, colors.default, true)
                } break
            }
        }
    }
}
async function drawPicture(ctx:CanvasRenderingContext2D, align:"left" | "right", params:{
    at:[number, number],
    size:number,
    backColor:string,
    textColor:string,
    stone:number,
    maxStone:number,
}, image:Buffer, username:string) {
    const alignLeft = align === "left"
    const {at, size, backColor, textColor} = params
    let [x,y] = at
    x = Math.floor(x)
    y = Math.floor(y)
    const tagWidth = Math.floor(size * 3)
    const tagHeight = Math.floor(size)
    const padding = Math.floor(size * 0.1)
    const pos = [x,y]
    // background
    ctx.fillStyle = backColor
    ctx.fillRect(x, y, tagWidth, tagHeight)
    const padPicSize = tagHeight - 2 * padding
    // move poistion
    if (alignLeft) {
        pos[0] += padding
        pos[1] += padding
    } else {
        pos[0] += tagWidth - padding - padPicSize
        pos[1] += padding
    }
    // image
    const picture = await sharp(image)
        .resize(padPicSize, padPicSize, { fit: "contain", position:"center", background: {r:0, g:0, b:0, alpha:0} })
        .toBuffer().then(loadImage)
    ctx.drawImage(picture,pos[0], pos[1])
    // image stroke
    ctx.strokeStyle = textColor
    ctx.lineWidth = 3
    ctx.fillStyle = "#111111"
    ctx.strokeRect(pos[0], pos[1], padPicSize, padPicSize)
    // ctx.fillRect(x + padPicture, y + padPicture, padPicSize, padPicSize)
    // move position
    const textWidth = Math.floor(tagWidth - (padPicSize + padding * 3))
    const fontSize = Math.floor((tagHeight - 3 * padding) / 2)
    if (alignLeft) {
        pos[0] += padding + padPicSize
    } else {
        pos[0] -= padding
    }
    pos[1] += fontSize
    if (!alignLeft) {
        ctx.textAlign = "right"
    }
    // nickname
    ctx.fillStyle = textColor
    ctx.font = `${fontSize}px NanumSquareRound`
    ctx.fillText(username, pos[0], pos[1],
        Math.floor(tagWidth - (padPicSize + padding * 3)))
    // thinking
    pos[1] = y + tagHeight - 2 * padding
    const getNum = (num:number) => {
        if (num == null || num < 0) {
            return "?"
        } else {
            return num.toString()
        }
    }
    ctx.fillText(`\u{25EF} ${getNum(params.stone)}/${getNum(params.maxStone)}`, pos[0], pos[1],
        Math.floor(tagWidth - (padPicSize + padding * 3)))
    return [tagWidth, tagHeight]
}
function loadImage(url:string | Buffer) {
    return new Promise<Image>((res, rej) => {
        const img = new Image()
        img.onload = () => res(img)
        img.onerror = (err:any) => {
            cLog.e(err)
            res(null)
        }
        img.src = url
    })
}
export function emojiAsSVG(emoji:string) {
    return `https://cdnjs.cloudflare.com/ajax/libs/twemoji/11.2.0/2/svg/${
        (emojiUnicode(emoji) as string).toLowerCase()}.svg`
}
function getFillString(str:string, length:number) {
    let s = ""
    for (let i = 0; i < length; i += 1) {
        s += str
    }
    return s
}