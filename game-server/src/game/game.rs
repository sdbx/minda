use model::EndedCause;
use model::GameRule;
use game::Cord;
use game::Stone;
use std::cmp::min;
use std::cmp::max;
use model::UserId;
use model::AxialCord;
use model::Task;
use game::{Player, Board};
use error::Error;

#[derive(Serialize, Deserialize, Clone, Debug)]
pub struct Move {
    pub player: Player,
    pub start: AxialCord,
    pub end: AxialCord,
    pub dir: AxialCord
}

#[derive(Clone, Debug)]
pub struct Game {
    pub map: String,
    pub board: Board,
    pub black: UserId,
    pub white: UserId,
    pub turn: Player,
    pub rule: GameRule,
    pub history: Vec<Move>,
    initial_stones: usize,
    // in ms
    pub current_time: usize,
    pub black_time: usize,
    pub white_time: usize
}

impl Game {
    pub fn new(black: UserId, white: UserId, map: &str, rule: GameRule) -> Option<Self> {
        let board = Board::from_string(map)?;
        let (stones, _) = board.count_stones();
        Some(Self {
            map: map.to_owned(),
            black: black,
            white: white,
            board: board,
            turn: Player::Black,
            rule: rule.clone(),
            initial_stones: stones,
            history: Vec::new(),
            current_time: rule.turn_timeout * 1000,
            black_time: rule.game_timeout * 1000,
            white_time: rule.game_timeout * 1000
        })
    }

    pub fn run_move(&mut self, id: UserId, start: AxialCord, end: AxialCord, dir: AxialCord) -> Result<(), Error> {
        let player = self.get_turn(id)?;

        self.history.push(Move {
            player: player,
            start: start,
            end: end,
            dir: dir
        });
        self.board.push(player, start.to_cord(), end.to_cord(), dir.to_cord())?;
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

    pub fn get_lose(&self) -> Option<(Player, EndedCause)>{
        if let Some(player) = self.get_stones_loser() {
            Some((player, EndedCause::LostStones))
        } else if let Some(player) = self.get_time_loser() {
            Some((player, EndedCause::Timeout))
        } else {
            None
        }
    }

    pub fn time_update(&mut self, dt: usize) -> bool {
        Game::sub_time(&mut self.current_time, dt);
        if self.turn == Player::Black {
            Game::sub_time(&mut self.black_time, dt);
        } else {
            Game::sub_time(&mut self.white_time, dt);
        }
        self.current_time % 1000 == 0 || self.black_time % 1000 == 0 || self.white_time % 1000 == 0 
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