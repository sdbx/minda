use server::Server;
use model::{Task, TaskResult};
use error::Error;

pub fn handle(server: &mut Server, task: Task) -> Result<String, Error> {
    Ok("".to_string())
}