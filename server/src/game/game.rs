use std::iter::FromIterator;
use std::collections::VecDeque;
use std::cmp::max;
use std::ops;

quick_error! {
    #[derive(Debug)]
    pub enum GameError {
        InvalidCord {
            description("Invalid coordinates")
        }
        InvalidVec {
            description("Invalid vec")
        }
    }
}

enum_number!(Stone {
    Blank = 0,
    Black = 1,
    White = 2,
});

pub enum Player {
    White,
    Black
}

impl Player {
    pub fn stone(&self) -> Stone {
        match self {
            Player::White => Stone::White,
            Player::Black => Stone::Black
        }
    }
}

#[derive(Serialize, Deserialize, Copy, Clone, Debug)]
pub struct Cord(isize, isize, isize);

#[derive(Serialize, Deserialize, Copy, Clone, Debug)]
pub struct Move {
    pub marble: Cord, 
    pub sel: Cord,
    pub dir: Cord,
}

impl Cord {
    const zero: Cord = Cord(0,0,0);

    pub fn is_linear_to(self, other: Cord) -> bool {
        let mut v = [(self.0-other.0).abs(),
            (self.1-other.1).abs(),
            (self.2-other.2).abs()];
        v.sort();
        v[0] == 0 && v[1] == v[2]
    }

    pub fn distance(self, other: Cord) -> usize {
        max(max((self.0-other.0).abs(), (self.1-other.1).abs()), (self.2-other.2).abs()) as usize
    }

    pub fn vec_size(self) -> usize {
        Cord::zero.distance(self)
    }

    pub fn is_linear_vec(self) -> bool {
        Cord::zero.is_linear_to(self)
    }

    pub fn linedraw(self, other: Cord) -> Vec<Cord> {
        let n = self.distance(other);
        let mut out: VecDeque<Cord> = VecDeque::new();
        for i in 0..n+1 {
            out.push_back(self.cube_lerp(other, 1.0 / n as f32 * i as f32));
        }
        Vec::from_iter(out.into_iter())
    }

    fn lerp(a: isize, b: isize, t: f32) -> isize {
        a + ((b - a) as f32 * t) as isize
    }

    fn cube_lerp(self, other: Cord, t: f32) -> Cord {
        Cord(Cord::lerp(self.0, other.0, t), 
                Cord::lerp(self.1, other.1, t),
                Cord::lerp(self.2, other.2, t))
    }

}

impl ops::Add<Cord> for Cord{
    type Output = Cord;

    fn add(self, _rhs: Cord) -> Cord {
        Cord(self.0 + _rhs.0, self.1 + _rhs.1, self.2 + _rhs.2)
    }
}

impl ops::Sub<Cord> for Cord{
    type Output = Cord;

    fn sub(self, _rhs: Cord) -> Cord {
        Cord(self.0 - _rhs.0, self.1 - _rhs.1, self.2 - _rhs.2)
    }
}

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
    payload: Vec<Vec<Stone>>,
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

    pub fn raw(&self) -> Vec<Vec<Stone>> {
        self.payload.clone()
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
        x > -self.side && x < self.side &&
        y > -self.side && y < self.side &&
        z > -self.side && z < self.side
    }

    fn to_axial(&self, cord: Cord) -> (usize, usize) {
        ((cord.0 + self.side - 1) as usize, (cord.1 + self.side - 1) as usize)
    }

    fn get_until_blank(&self, p: Cord, dir: Cord) -> Result<Vec<Stone>, GameError> {
        let mut i = p;
        let mut out: VecDeque<Stone> = VecDeque::new();
        loop {
            let res = self.get(i);
            match res {
                Ok(s) => {
                    if s == Stone::Blank {
                        break;
                    }
                    out.push_back(s);
                },
                Err(_) => break
            }
            i = i + dir;
        }
        Ok(Vec::from_iter(out.into_iter()))
    }

    fn get_between(&self, from: Cord, to: Cord) -> Result<Vec<Stone>, GameError> {
        if !self.validate(from) || !self.validate(to) {
            return Err(GameError::InvalidCord)
        }
        if !from.is_linear_to(to) {
            return Err(GameError::InvalidCord)
        }
        let cords = from.linedraw(to);
        Ok(cords.iter().map(|&c| { 
            self.get(c).unwrap()
        }).collect())
    }

    fn push(&mut self, player: Player, from: Cord, to: Cord, dir: Cord) -> Result<(), GameError> {
        if !self.validate(from) || !self.validate(to) {
            return Err(GameError::InvalidCord)
        }
        if !from.is_linear_to(to) || from.distance(to) > 2 {
            return Err(GameError::InvalidCord)
        }
        if !dir.is_linear_vec() || dir.vec_size() != 1 {
            return Err(GameError::InvalidVec)
        }


        Ok(())
    }
}
