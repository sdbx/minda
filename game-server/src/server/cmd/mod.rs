use model::Event;
use server::room::Room;
use server::{Server, Connection};
use uuid::Uuid;
use model::Command;
use error::Error;

mod common;
mod game;
mod room;

pub fn handle(server: &mut Server, conn: &Connection, cmd: &Command) -> Result<(), Error> {
    match cmd {
        Command::Connect { invite } => {
            common::connect(server, conn, &invite)
        },
        Command::Chat { content } => {
            common::chat(server, conn, &content)
        },
        Command::Conf { conf } => {
            room::conf(server, conn, &conf)
        },
        Command::Move { start, end, dir } => {
            game::game_move(server, conn, *start, *end, *dir)
        },
        Command::Start { } => {
            room::start(server, conn)
        },
        Command::Gg { } => {
            game::game_gg(server, conn)
        },
        Command::Ban { user } => {
            room::ban(server, conn, *user)
        }
    }
}