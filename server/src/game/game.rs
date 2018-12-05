use game::cord::Cord;
use game::{Board};
use std::fmt::{self, Display, Formatter};

quick_error! {
    #[derive(Debug)]
    pub enum GameError {
        InvalidCord {
            description("Invalid coordinates")
        }
        InvalidVec {
            description("Invalid vec")
        }
        InvalidMove {
            description("Invalid move")
        }
    }
}

enum_number!(Stone {
    Blank = 0,
    Black = 1,
    White = 2,
});

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

pub struct Game {
    pub board: Board,
}

impl Game {
    pub fn new(board: Board) -> Self {
        Self {
            board: board
        }
    }
}

pub struct Move {
    pub player: Player,
    pub from: Cord,
    pub to: Cord,
    pub dir: Cord
}