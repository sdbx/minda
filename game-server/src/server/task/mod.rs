use server::Server;
use model::{Task, TaskResult};
use error::Error;

mod create_room;
mod join_room;

pub fn handle(server: &mut Server, task: Task) -> Result<String, Error> {
    match task {
        Task::CreateRoom { user, conf } => {
            create_room::handle(server, user, conf)
        },
        Task::JoinRoom { user, room } => {
            join_room::handle(server, user, room)
        },
        _ => Err(Error::Internal)
    }
}