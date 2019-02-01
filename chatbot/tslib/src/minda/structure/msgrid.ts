import { DeepReadonly } from "../../types/deepreadonly"

export class MSGrid {
    public readonly centerPosition:number
    public readonly sqaureSize:number
    public readonly decodedGrid:DeepReadonly<StoneType[][]>
    protected grid:StoneType[][]
    public constructor(board:MSGridEncode) {
        this.grid = board.split("#").map((v) => v.split("@").map((v2) => Number.parseInt(v2)))
        let pastSize = this.grid.length
        for (const size of this.grid.map((v) => v.length)) {
            if (pastSize !== size) {
                throw new Error("Decode ERROR!")
            }
            pastSize = size
        }
        this.sqaureSize = pastSize
        this.centerPosition = Math.floor(this.sqaureSize / 2)
        const dGrid:StoneType[][] = []
        for (let i = 0; i < this.sqaureSize; i += 1) {
            const row:StoneType[] = []
            for (let k = 0; k < this.sqaureSize; k += 1) {
                row.push(this.grid[k][i])
            }
            if (i < this.centerPosition) {
                row.splice(0, this.sqaureSize - this.getWidth(i))
            } else if (i > this.centerPosition) {
                row.splice(this.getWidth(i), this.sqaureSize - this.getWidth(i))
            }
            dGrid.push(row)
        }
        this.decodedGrid = dGrid
        console.log(this.decodedGrid)
    }
    public getRow(index:number, side:"black" | "white") {
        if (side === "white") {
            index = this.sqaureSize - index - 1
        }
        return this.decodedGrid[index]
    }
    public getWidth(index:number) {
        if (index < 0 || index >= this.sqaureSize) {
            return -1
        }
        const delta = Math.abs(this.centerPosition - index)
        return this.sqaureSize - delta
    }
    public getStones(color:"black" | "white" | "void") {
        const stones:[number, number, number] = [0, 0, 0]
        for (const row of this.decodedGrid) {
            for (const el of row) {
                if (el === StoneType.black) {
                    stones[0] += 1
                } else if (el === StoneType.white) {
                    stones[1] += 1
                } else if (el === StoneType.void) {
                    stones[2] += 1
                }
            }
        }
        switch (color) {
            case "black":
                return stones[0]
            case "white":
                return stones[1]
            case "void":
                return stones[2]
        }
        return 0
    }
}
export type MSGridEncode = string
export enum StoneType {
    void = 0,
    black = 1,
    white = 2,
}