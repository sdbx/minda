use server::Server;
use model::{Task, TaskResult};
use error::Error;

mod delete_room;
mod create_room;
mod kick_user;
mod join_room;

pub fn handle(server: &mut Server, task: Task) -> Result<String, Error> {
    match task {
        Task::CreateRoom { room_id, user_id, conf } => {
            create_room::handle(server, &room_id, user_id, &conf)
        },
        Task::JoinRoom { user_id, room_id } => {
            join_room::handle(server, user_id, &room_id)
        },
        Task::KickUser { user_id, room_id } => {
            kick_user::handle(server, user_id, &room_id)
        },
        Task::DeleteRoom { room_id } => {
            delete_room::handle(server, &room_id)
        },
        _ => { Err(Error::Internal) }
    }
}