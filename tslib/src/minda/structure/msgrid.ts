export class MSGrid {
    public readonly centerPosition:number
    public readonly sqaureSize:number
    protected decodedGrid:StoneType[][]
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
        this.decodedGrid = []
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
            this.decodedGrid.push(row)
        }
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
}
export type MSGridEncode = string
export enum StoneType {
    void = 0,
    black = 1,
    white = 2,
}