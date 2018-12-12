use server::Server;
use model::{Task, TaskResult};
use error::Error;

mod create_room;

pub fn handle(server: &mut Server, task: Task) -> Result<String, Error> {
    match task {
        Task::CreateRoom { name, user } => {
            create_room::handle(server, &name)
        },
        _ => Err(Error::Internal)
    }
}