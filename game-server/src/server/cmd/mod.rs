use model::Event;
use server::room::Room;
use server::{Server, Connection};
use uuid::Uuid;
use model::Command;
use std::error::{Error as SError};
use error::Error;

mod common;
mod game;
mod room;

pub fn handle(server: &mut Server, conn: &Connection, cmd: &Command) -> Result<(), Error> {
    match cmd {
        Command::Connect { invite } => {
            let out = common::connect(server, conn, &invite);
            if out.is_err() {
                return Err(Error::ShouldTerminate(out.err().unwrap().description().to_owned()))
            }
            out
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