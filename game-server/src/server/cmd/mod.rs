use model::Event;
use server::room::Room;
use server::{Server, Connection};
use uuid::Uuid;
use model::Command;

mod game;
mod connect;

pub fn handle(server: &mut Server, conn: &Connection, cmd: Command) {
    match cmd {
        Command::Connect { id } => {
            connect::handle(server, conn, id);
        },
        Command::Move { start, end, dir } => {{
            game::game_move(server, conn, start, end, dir);
        }}
    }
}