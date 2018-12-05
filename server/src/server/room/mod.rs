use game::Game;
use std::collections::HashSet;

pub struct Room {
    pub name: String,
    pub users: HashSet<String>,
    pub ingame: bool,
    pub game: Game
}