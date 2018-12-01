quick_error! {
    #[derive(Debug)]
    pub enum GameError {
        InvalidCord {
            description("Invalid coordinates")
        }
    }
}

enum_number!(Stone {
    Blank = 0,
    Black = 1,
    White = 2,
});

#[derive(Serialize, Deserialize, Copy, Clone, Debug)]
pub struct Cord(isize, isize, isize);

pub struct Game {
    pub board: Board,
    pub black: String,
    pub white: String
}

impl Game {
    pub fn new(board: Board, black: String, white: String) -> Self {
        Self {
            board: board,
            black: black,
            white: white
        }
    }
}

#[derive(Clone)]
pub struct Board {
    pub payload: Vec<Vec<Stone>>,
    side: isize
}

impl Board {
    pub fn new(side: usize) -> Self {
        use self::Stone::*;
        Self {
            payload: vec![vec![Blank; side*2-1]; side*2-1],
            side: side as isize,
        }
    }

    pub fn test_board() -> Self {
        let mut board = Board::new(5);
        board.set(Cord(0,0,0), Stone::Black);
        board.set(Cord(1,0,-1), Stone::White);
        board
    }

    pub fn get(&self, cord: Cord) -> Result<Stone, GameError> {
        if !self.validate(cord) {
            return Err(GameError::InvalidCord);
        }
        let (i, j) = self.to_axial(cord);
        Ok(self.payload[i][j])
    }

    pub fn set(&mut self, cord: Cord, stone: Stone) -> Result<(), GameError> {
        if !self.validate(cord) {
            return Err(GameError::InvalidCord)
        }
        let (i, j) = self.to_axial(cord);
        self.payload[i][j] = stone;
        Ok(())
    }

    fn validate(&self, cord: Cord) -> bool {
        let Cord(x, y, z) = cord;
        x <= -self.side || x >= self.side ||
        y <= -self.side || y >= self.side ||
        z <= -self.side || z >= self.side
    }

    fn to_axial(&self, cord: Cord) -> (usize, usize) {
        ((cord.0 + self.side - 1) as usize, (cord.1 + self.side - 1) as usize)
    }
}
