import { createCanvas, Image, loadImage } from "canvas"
import emojiUnicode from "emoji-unicode"
import fs from "fs-extra"
import { MSGrid, StoneType } from "minda-ts"
import fetch from "node-fetch"
import sharp from "sharp"
import { debugPath } from "../snow/config/snowconfig"
import awaitEvent from "../timeout"
import { blank1_1, blank1_2, blank1_3, blank1_4, blank1_6, blankChar, blankChar2 } from "./cbconst"

export async function renderBoard(board:MSGrid,
    blackImage = emojiAsSVG("\u{26AB}"),
    whiteImage = emojiAsSVG("\u{26AA}"),
    noStoneImage = emojiAsSVG("\u{1F535}")) {

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
        default: parseColor(noStoneImage, "#777777"),
    }
    const elSize = 1 / 3
    // the width of hexagon (l)
    const hexaLength = 1200
    // 1 * l / (2n)
    const oneX = hexaLength / (2 * board.sqaureSize)
    // 3^(1/2)l / (2n)
    const oneY = Math.sqrt(3) * hexaLength / (2 * board.sqaureSize)
    // stone/image width (not radios but width)
    const elementWidth = 2 * hexaLength * elSize / board.sqaureSize
    // hexagon width
    // const hexaWidth = Math.ceil(hexaLength + elementWidth)
    // hexagon height
    // const hexaHeight = Math.ceil(Math.sqrt(3) * hexaLength / 2 + elementWidth)
    // canvas width
    const canvasWidth = Math.ceil((hexaLength + elementWidth) * 1.335)
    // canvas height
    const canvasHeight = Math.ceil(((Math.sqrt(3) * hexaLength / 2) + elementWidth) * 1.332)
    // the center point of hexagon
    const centerPoint = [canvasWidth / 2, canvasHeight / 2]

    const frame = await sharp(
        await fs.readFile(`${debugPath}/board.png`))
        .resize(canvasWidth, canvasHeight)
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
    // draw background
    if (frame != null) {
        ctx.drawImage(frame, 0, 0)
    }
    /**
     * Load image
     */
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
            .overlayWith(roundedCorners, {cutout: true})
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
    const buffer:Buffer = canvas.toBuffer()
    return buffer
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