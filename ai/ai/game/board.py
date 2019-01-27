class Board:
    def __init__(self, side: int):
        self.side = side
        self.payload = [[0 for x in range(side*2-1)] for y in range(side*2-1)] 