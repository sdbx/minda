import { createCanvas, Image, loadImage, registerFont } from "canvas"
import emojiUnicode from "emoji-unicode"
import fs from "fs-extra"
import sizeOf from "image-size"
<<<<<<< HEAD
import { MSGrid, StoneType } from "minda-ts"
=======
import { MSGrid, MSUser, StoneType } from "minda-ts"
>>>>>>> 5266d47db47c99f4d5cd0fd64c2491ee1015895e
import fetch from "node-fetch"
import sharp from "sharp"
import { debugPath } from "../snow/config/snowconfig"
import awaitEvent from "../timeout"
import { blank1_1, blank1_2, blank1_3, blank1_4, blank1_6, blankChar, blankChar2 } from "./cbconst"

export async function renderBoard(board:MSGrid,
    blackImage:string = "",
    whiteImage:string = "",
    noStoneImage:string = "") {
<<<<<<< HEAD

=======
    // load font
    const ttf = `${debugPath}/NanumSquareRoundR.ttf`
    registerFont(ttf, { family: "NanumSquareRound" })
    // define size
    const hexagonSize = 1500
    const frameWidth = Math.floor(hexagonSize * 1.2211)
    const circleFR = Math.floor(frameWidth * 1.1)
    const canvas = createCanvas(
        circleFR,
        circleFR, "PDF")
    // load frame
    const frameBuffer = await fs.readFile(`${debugPath}/board.png`)
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
    await drawHexagon(ctx, [Math.floor(circleFR / 2), Math.floor(circleFR / 2)], hexagonSize, "#ffefbc", {
        board,
        blackImage,
        whiteImage,
        noStoneImage,
    })
    const infoSize = Math.floor(hexagonSize / 8)
    await drawPicture(ctx, {
        at: [(circleFR - infoSize * 3) / 2, 0],
        size: infoSize,
        backColor: "#222222",
        textColor: "#eeeeee"
    }, frameBuffer, "Black")
    await drawPicture(ctx, {
        at: [(circleFR - infoSize * 3) / 2, circleFR - infoSize],
        size: infoSize,
        backColor: "#eeeeee",
        textColor: "#222222"
    }, frameBuffer, "White")
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
>>>>>>> 5266d47db47c99f4d5cd0fd64c2491ee1015895e
    const parseColor = (str:string, fault:string) => {
        if (str.startsWith("#") && /^#[0-9A-Fa-f]{6}$/ig.test(str)) {
            return str.toUpperCase()
        } else {
            return fault.toUpperCase()
        }
    }
    const colors = {
        black: parseColor(blackImage, "#eeeeee"),
        white: parseColor(whiteImage, "#111111"),
<<<<<<< HEAD
        default: parseColor(noStoneImage, "#f7deb4"),
=======
        default: parseColor(noStoneImage, "#ddd3b3"),
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
>>>>>>> 5266d47db47c99f4d5cd0fd64c2491ee1015895e
    }
    /* second: define constans */
    hexaLength = Math.floor(hexaLength * board.sqaureSize / (board.sqaureSize + Math.sqrt(2) - 1))
    // 1 * l / (2n)
    const oneX = hexaLength / (2 * board.sqaureSize)
    // 3^(1/2)l / (2n)
    const oneY = Math.sqrt(3) * hexaLength / (2 * board.sqaureSize)
    // stone/image width (not radios but width)
    const elementWidth = 2 * hexaLength * elSize / board.sqaureSize
<<<<<<< HEAD
    // hexagon width
    // const hexaWidth = Math.ceil(hexaLength + elementWidth)
    // hexagon height
    // const hexaHeight = Math.ceil(Math.sqrt(3) * hexaLength / 2 + elementWidth)
    // canvas width
    // 1.335 : 1.332
    const canvasWidth = Math.ceil((hexaLength + elementWidth) * 1.2)
    // canvas height
    const canvasHeight = Math.ceil(((Math.sqrt(3) * hexaLength / 2) + elementWidth) * 1.2)
    // the center point of hexagon
    const centerPoint = [canvasWidth / 2, canvasHeight / 2]
    // layout 2: frame
    const frame = await sharp(
        await fs.readFile(`${debugPath}/board.png`))
        .resize(canvasWidth, canvasHeight, {fit: "inside"})
        .toBuffer().then((bf) => {
            return new Promise<Image>((res, rej) => {
                const img = new Image()
                img.onload = () => res(img)
                img.onerror = (err:any) => { res(null) }
                img.src = bf
            })
        })
    // canvas
    const canvas = createCanvas(
        canvasWidth, 
        canvasHeight, "PDF")
    // context
    const ctx:CanvasRenderingContext2D = canvas.getContext("2d")
    // draw circle
    
    // draw background
    if (frame != null) {
        ctx.drawImage(frame, 0, 0)
    }
    /**
     * Load image
     */
=======
    /* third: draw */
    // load image
>>>>>>> 5266d47db47c99f4d5cd0fd64c2491ee1015895e
    const getImage = async (url:string) => {
        if (!url.startsWith("http")) {
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
            const drawCircle = (fillColor:string) => {
                ctx.beginPath()
                ctx.arc(x, y, elementWidth / 2, 0, 2 * Math.PI, false)
                ctx.fillStyle = fillColor
                ctx.fill()
            }
            const drawImage = (fillImage:Image, cornerColor:string) => {
                ctx.drawImage(fillImage,
                    Math.floor(x - elementWidth / 2),
                    Math.floor(y - elementWidth / 2),
                    Math.round(elementWidth), Math.round(elementWidth))
                ctx.beginPath()
                ctx.arc(x, y, elementWidth / 2, 0, 2 * Math.PI, false)
                ctx.strokeStyle = cornerColor
                ctx.lineWidth = 3
                ctx.stroke()
            }
            const draw = (fillImage:Image, fillColor:string) => {
                if (fillImage != null) {
                    drawImage(fillImage, "#333333")
                } else {
                    drawCircle(fillColor)
                }
            }
            switch (stone) {
                case StoneType.black: {
                    draw(images.black, colors.black)
                } break
                case StoneType.white: {
                    draw(images.white, colors.white)
                } break
                case StoneType.void: {
                    draw(images.default, colors.default)
                } break
            }
        }
    }
<<<<<<< HEAD
    const buffer:Buffer = canvas.toBuffer()
    /**
     * Background circle
     */
    const frameSize = sizeOf(buffer)
    const frameImg = await new Promise<Image>((res, rej) => {
        const img = new Image()
        img.onload = () => res(img)
        img.onerror = (err:any) => { res(null) }
        img.src = buffer
    })
    const padSize = Math.floor(Math.max(frameSize.width, frameSize.height) * 1.05)
    const outerCanvas = createCanvas(padSize, padSize, "PDF")
    const outerCtx:CanvasRenderingContext2D = outerCanvas.getContext("2d")
    outerCtx.beginPath()
    const r = padSize / 2
    outerCtx.arc(r, r, r, 0, 2 * Math.PI, false)
    outerCtx.fillStyle = "#f9edca"
    outerCtx.fill()
    outerCtx.drawImage(frameImg, (padSize - frameSize.width) / 2, (padSize - frameSize.height) / 2)
    const out = outerCanvas.toBuffer()
    return out
=======
}
async function drawPicture(ctx:CanvasRenderingContext2D, params:{
    at:[number, number],
    size:number,
    backColor:string,
    textColor:string,
}, image:Buffer, username:string) {
    const {at, size, backColor, textColor} = params
    let [x,y] = at
    x = Math.floor(x)
    y = Math.floor(y)
    const tagWidth = Math.floor(size * 3)
    const tagHeight = Math.floor(size)
    const padPicture = Math.floor(size * 0.1)
    // background
    ctx.fillStyle = backColor
    ctx.fillRect(x, y, tagWidth, tagHeight)
    // image
    const padPicSize = tagHeight - 2 * padPicture
    const picture = await sharp(image)
        .resize(padPicSize, padPicSize, { fit: "contain", position:"center", background: {r:0, g:0, b:0, alpha:0} })
        .toBuffer().then(loadImage)
    ctx.drawImage(picture, x + padPicture, y + padPicture)
    // image stroke
    ctx.strokeStyle = textColor
    ctx.lineWidth = 3
    ctx.fillStyle = "#111111"
    ctx.strokeRect(x + padPicture, y + padPicture, padPicSize, padPicSize)
    // ctx.fillRect(x + padPicture, y + padPicture, padPicSize, padPicSize)
    // nickname
    ctx.fillStyle = textColor
    const fontSize = Math.floor(tagHeight / 2 - 2 * padPicture)
    ctx.font = `${fontSize}px NanumSquareRound`
    ctx.fillText(username, x + padPicSize + padPicture * 3, Math.floor(y + padPicture * 2 + fontSize / 2),
        Math.floor(tagWidth - (padPicSize + padPicture * 3)))
    return [tagWidth, tagHeight]
}
function loadImage(url:string | Buffer) {
    return new Promise<Image>((res, rej) => {
        const img = new Image()
        img.onload = () => res(img)
        img.onerror = (err:any) => {
            console.log(err)
            res(null)
        }
        img.src = url
    })
>>>>>>> 5266d47db47c99f4d5cd0fd64c2491ee1015895e
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