use model::EndedCause;
use model::GameRule;
use game::Cord;
use game::Stone;
use std::cmp::min;
use std::cmp::max;
use model::UserId;
use model::AxialCord;
use game::{Player, Board, Move};
use error::Error;

pub struct Game {
    pub board: Board,
    pub black: UserId,
    pub white: UserId,
    pub turn: Player,
    pub rule: GameRule,
    initial_stones: usize,
    // in ms
    current_time: usize,
    black_time: usize,
    white_time: usize
}

impl Game {
    pub fn new(black: UserId, white: UserId, board: Board, rule: GameRule) -> Self {
        let (stones, _) = board.count_stones();
        Self {
            board: board,
            black: black,
            white: white,
            turn: Player::Black,
            rule: rule,
            initial_stones: stones,
            current_time: rule.turn_timeout * 1000,
            black_time: rule.game_timeout * 1000,
            white_time: rule.game_timeout * 1000
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
        self.current_time = self.rule.turn_timeout;
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

    pub fn get_loser(&self) -> Option<(Player, EndedCause)>{
        if let Some(player) = self.get_stones_loser() {
            Some((player, EndedCause::LostStones))
        } else if let Some(player) = self.get_time_loser() {
            Some((player, EndedCause::Timeout))
        } else {
            None
        }
    }

    pub fn time_update(&mut self, dt: usize) {
        Game::sub_time(&mut self.current_time, dt);
        if self.turn == Player::Black {
            Game::sub_time(&mut self.black_time, dt);
        } else {
            Game::sub_time(&mut self.white_time, dt);
        }
    }

    fn sub_time(time: &mut usize, dt: usize) {
        if *time < dt {
            *time = 0;
        } else {
            *time -= dt;
        }
    }

    fn get_stones_loser(&self) -> Option<Player> {
        let (black, white) = self.board.count_stones();
        if self.initial_stones - black >= self.rule.defeat_lost_stones {
            Some(Player::Black)
        } else if self.initial_stones - white >= self.rule.defeat_lost_stones {
            Some(Player::White)
        } else {
            None
        }
    }

    fn get_time_loser(&self) -> Option<Player> {
        if self.black_time <= 0 {
            Some(Player::Black)
        } else if self.white_time <= 0 {
            Some(Player::White)
        } else if self.current_time <= 0 {
            Some(self.turn)
        } else {
            None
        }
    }
}