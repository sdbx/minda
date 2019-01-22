use model::Event;
use server::room::Room;
use server::{Server, Connection};
use uuid::Uuid;
use model::Command;
use error::Error;

mod conf;
mod chat;
mod game;
mod connect;
mod start;

pub fn handle(server: &mut Server, conn: &Connection, cmd: &Command) -> Result<(), Error> {
    match cmd {
        Command::Connect { invite } => {
            connect::handle(server, conn, &invite)
        },
        Command::Chat { content } => {
            chat::handle(server, conn, &content)
        },
        Command::Conf { conf } => {
            conf::handle(server, conn, &conf)
        },
        Command::Move { start, end, dir } => {
            game::game_move(server, conn, *start, *end, *dir)
        },
        Command::Start { } => {
            start::handle(server, conn)
        },
        Command::Gg { } => {

        },
        Command::Ban { user } => {

        }
    }
}