//TODO: move to board

use model::AxialCord;
use board::Player;
use board::Board;
use board::BoardError;
use board::Move;

pub struct Game {
    pub board: Board,
    pub black: String,
    pub white: String,
    pub turn: Player
}

impl Game {
    pub fn run_move(&mut self, id: &str, start: AxialCord, end: AxialCord, dir: AxialCord) -> Result<(), BoardError> {
        let player = self.get_turn(id)?;

        self.board.push(Move {
            player: player,
            from: start.to_cord(),
            to: end.to_cord(),
            dir: dir.to_cord()
        })
    }

    pub fn get_turn(&self, id: &str) -> Result<Player, BoardError> {
        if self.black == id && self.turn == Player::Black {
            Ok(Player::Black)
        } else if self.white == id && self.turn == Player::White {
            Ok(Player::White)
        } else {
            Err(BoardError::InvalidMove)
        }
    }
}