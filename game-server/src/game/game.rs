use model::UserId;
use model::AxialCord;
use game::{Player, Board, Move};
use error::Error;

pub struct Game {
    pub board: Board,
    pub black: UserId,
    pub white: UserId,
    pub turn: Player
}

impl Game {
    pub fn new(black: UserId, white: UserId, board: Board) -> Self {
        Self {
            board: board,
            black: black,
            white: white,
            turn: Player::Black
        }
    }
    pub fn run_move(&mut self, id: UserId, start: AxialCord, end: AxialCord, dir: AxialCord) -> Result<(), Error> {
        let player = self.get_turn(id)?;

        self.board.push(Move {
            player: player,
            from: start.to_cord(),
            to: end.to_cord(),
            dir: dir.to_cord()
        })?;
        self.turn = self.turn.opp();
        Ok(())
    }

    pub fn get_turn(&self, id: UserId) -> Result<Player, Error> {
        if self.black == id && self.turn == Player::Black {
            Ok(Player::Black)
        } else if self.white == id && self.turn == Player::White {
            Ok(Player::White)
        } else {
            Err(Error::InvalidMove)
        }
    }
}