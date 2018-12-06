use std::collections::VecDeque;
use std::iter::FromIterator;
use game::{Player, Stone, Move, GameError, Cord};

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

    fn nums_of_stones(player: Player, stones: Vec<Stone>) -> Option<(usize, usize)> {
        let mut opp_started = false;
        let mut me = 0;
        let mut opp = 0;
        if stones[0] != player.stone() {
            return None;
        }
        for s in stones.iter() {
            if *s == player.stone() {
                if opp_started {
                    return None;
                }
                me += 1;
            } else {
                opp_started = true;
                opp += 1;
            }
        }
        return Some((me, opp))
    }

    fn push_foward(&mut self, player: Player, from: Cord, dir: Cord) -> Result<(), GameError> {
        match self.get_until_blank(from, dir) {
            Ok(stones) => {
                match Board::nums_of_stones(player, stones) {
                    Some((me, opp)) => {
                        if me <= opp {
                            return Err(GameError::InvalidMove)
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
                    },
                    None => Err(GameError::InvalidMove)
                }
            }
            Err(e) => Err(e)
        }
    }

    fn push_sideways(&mut self, player: Player, from: Cord, to: Cord, dir: Cord) -> Result<(), GameError> {
        let cords = from.linedraw(to);
        for cord in cords.iter() {
            match self.get(*cord) {
                Ok(s) => {
                    if s != player.stone() {
                        return Err(GameError::InvalidMove)
                    }
                },
                Err(_) => return Err(GameError::InvalidMove)
            }
            if self.validate(*cord + dir) {
                match self.get(*cord + dir) {
                    Ok(s) => {
                        if s != Stone::Blank {
                            return Err(GameError::InvalidMove)
                        }
                    },
                    Err(_) => return Err(GameError::InvalidMove)
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

    fn push_one(&mut self, player: Player, from: Cord, dir: Cord) -> Result<(), GameError> {
        if self.validate(from + dir) {
            match self.get(from + dir) {
                Ok(s) => {
                    if s != Stone::Blank {
                        return Err(GameError::InvalidMove)
                    }
                },
                Err(e) => return Err(e)
            }
        }
        if self.validate(from + dir) {
            self.set(from + dir, player.stone()).unwrap();
        }
        self.set(from, Stone::Blank).unwrap();
        Ok(())
    }

    fn push_internal(&mut self, player: Player, from: Cord, to: Cord, dir: Cord) -> Result<(), GameError> {
        if !self.validate(from) || !self.validate(to) {
            return Err(GameError::InvalidCord)
        }
        if !dir.is_linear_vec() || dir.vec_size() != 1 {
            return Err(GameError::InvalidVec)
        }
        if from == to {
            return self.push_one(player, from, dir)
        }
        if !from.is_linear_to(to) || from.distance(to) > 2 {
            return Err(GameError::InvalidCord)
        }
        if from.dir(to) == dir {
            return self.push_foward(player, from, dir)
        } else if from.dir(to) == -dir {
            return self.push_foward(player, to, dir)
        } else {
            return self.push_sideways(player, from, to, dir)
        }
    }

    pub fn push(&mut self, m: Move) -> Result<(), GameError> {
        self.push_internal(m.player, m.from, m.to, m.dir)
    }
}