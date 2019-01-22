use std::cmp::min;
use std::cmp::max;
use std::collections::VecDeque;
use std::iter::FromIterator;
use std::fmt::{self, Display, Formatter};
use game::Cord;
use error::Error;

enum_number!(Stone {
    Blank = 0,
    Black = 1,
    White = 2,
});

impl Stone {
    pub fn from_num(i: usize) -> Option<Self> {
        match i {
            0 => Some(Stone::Blank),
            1 => Some(Stone::Black),
            2 => Some(Stone::White),
            _ => None
        }
    }
}
impl Display for Stone {
    fn fmt(&self, f: &mut Formatter) -> fmt::Result {
        use self::Stone::*;
        match *self {
            Blank => write!(f, "O"),
            Black => write!(f, "B"),
            White => write!(f, "W")
        }
    }
}

#[derive(PartialEq)]
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

    pub fn opp(&self) -> Player {
        match self {
            Player::White => Player::Black,
            Player::Black => Player::White
        }
    }

    pub fn to_string(&self) -> String {
        match self {
            Player::White => "white".to_owned(),
            Player::Black => "black".to_owned()
        }
    }
}

pub struct Move {
    pub player: Player,
    pub from: Cord,
    pub to: Cord,
    pub dir: Cord
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

    pub fn from_string(msg: &str) -> Option<Self> {
        let parsed = msg.split("#")
            .map(|s| s.split("@")
                .collect::<Vec<_>>()
            ).collect::<Vec<_>>();
        if parsed.len() <= 1 {
            return None
        }
        let mut out = Board::new((parsed.len() + 1) / 2);
        for i in 0..parsed.len() {
            for j in 0..parsed[i].len() {
                if i >= out.payload.len() || j >= out.payload[i].len() {
                    return None
                }
                let num = match parsed[i][j].parse::<usize>() {
                    Ok(n) => n,
                    Err(_) => return None
                };
                out.payload[i][j] = Stone::from_num(num)?;
            }
        }
        Some(out)
    }

    pub fn side(&self) -> isize {
        return self.side
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

    pub fn get(&self, cord: Cord) -> Result<Stone, Error> {
        if !self.validate(cord) {
            return Err(Error::InvalidCord);
        }
        let (i, j) = self.to_axial(cord);
        Ok(self.payload[i][j])
    }

    pub fn set(&mut self, cord: Cord, stone: Stone) -> Result<(), Error> {
        if !self.validate(cord) {
            return Err(Error::InvalidCord)
        }
        let (i, j) = self.to_axial(cord);
        self.payload[i][j] = stone;
        Ok(())
    }

    // (black, white)
    pub fn count_stones(&self) -> (usize, usize) {
        let N = self.side();
        let mut black = 0;
        let mut white = 0;
        for x in -N..N+1 {
            for y in max(-N, -x-N)..min(N, -x+N)+1 {
                let z = -x-y;
                let stone = self.get(Cord(x,y,z)).unwrap();
                match stone {
                    Stone::Black => { black += 1; },
                    Stone::White => { white += 1; },
                    _ => { },
                }
            }
        }
        (black, white)
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

    fn get_until_blank(&self, p: Cord, dir: Cord) -> Result<Vec<Stone>, Error> {
        let mut i = p;
        let mut out: VecDeque<Stone> = VecDeque::new();
        loop {
            match self.get(i) {
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

    fn get_between(&self, from: Cord, to: Cord) -> Result<Vec<Stone>, Error> {
        if !self.validate(from) || !self.validate(to) {
            return Err(Error::InvalidCord)
        }
        if !from.is_linear_to(to) {
            return Err(Error::InvalidCord)
        }
        let cords = from.linedraw(to);
        Ok(cords.iter().map(|&c| { 
            self.get(c).unwrap()
        }).collect())
    }

    fn nums_of_stones(player: Player, stones: Vec<Stone>) -> Result<(usize, usize), Error> {
        let mut opp_started = false;
        let mut me = 0;
        let mut opp = 0;
        if stones[0] != player.stone() {
            return Err(Error::InvalidMove);
        }
        for s in stones.iter() {
            if *s == player.stone() {
                if opp_started {
                    return Err(Error::InvalidMove);
                }
                me += 1;
            } else {
                opp_started = true;
                opp += 1;
            }
        }
        return Ok((me, opp))
    }

    fn push_foward(&mut self, player: Player, from: Cord, dir: Cord) -> Result<(), Error> {
        let stones = self.get_until_blank(from, dir)?;
        let (me, opp) = Board::nums_of_stones(player, stones)?;
        if me <= opp {
            return Err(Error::InvalidMove)
        }
        let mut i = from + dir * (me + opp) as isize;
        while i != from {
            let s = self.get(i - dir).unwrap();
            if self.validate(i) {
                self.set(i, s).unwrap();
            }
            i = i - dir;
        }
        self.set(from, Stone::Blank).unwrap();
        Ok(())
    }

    fn push_sideways(&mut self, player: Player, from: Cord, to: Cord, dir: Cord) -> Result<(), Error> {
        let cords = from.linedraw(to);
        for cord in cords.iter() {
            if self.validate(*cord + dir) {
                let s = self.get(*cord + dir)?; 
                if s != Stone::Blank {
                    return Err(Error::InvalidMove)
                }
            }
        }
        for cord in cords.iter() {
            if self.validate(*cord + dir) {
                self.set(*cord + dir, player.stone()).unwrap();
            }
            self.set(*cord, Stone::Blank).unwrap();
        }
        Ok(())
    }

    fn push_one(&mut self, player: Player, from: Cord, dir: Cord) -> Result<(), Error> {
        if self.validate(from + dir) {
            let s = self.get(from + dir)?;
            if s != Stone::Blank {
                return Err(Error::InvalidMove)
            }
        }
        if self.validate(from + dir) {
            self.set(from + dir, player.stone()).unwrap();
        }
        self.set(from, Stone::Blank).unwrap();
        Ok(())
    }

    fn push_internal(&mut self, player: Player, from: Cord, to: Cord, dir: Cord) -> Result<(), Error> {
        if !self.validate(from) || !self.validate(to) {
            return Err(Error::InvalidCord)
        }
        if !dir.is_linear_vec() || dir.vec_size() != 1 {
            return Err(Error::InvalidVec)
        }
        if from == to {
            return self.push_one(player, from, dir)
        }
        if !from.is_linear_to(to) || from.distance(to) > 2 {
            return Err(Error::InvalidCord)
        }
        let cords = from.linedraw(to);
        for cord in cords.iter() {
            let s = self.get(*cord)?;
            if s != player.stone() {
                return Err(Error::InvalidMove)
            }
        }
        if from.dir(to) == dir {
            return self.push_foward(player, from, dir)
        } else if from.dir(to) == -dir {
            return self.push_foward(player, to, dir)
        } else {
            return self.push_sideways(player, from, to, dir)
        }
    }

    pub fn push(&mut self, m: Move) -> Result<(), Error> {
        self.push_internal(m.player, m.from, m.to, m.dir)
    }
}