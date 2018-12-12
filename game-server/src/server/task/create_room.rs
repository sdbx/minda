use server::{Server, ServerEvent};
use model::{Task, TaskResult, CreateRoomResult};
use error::Error;
use server::room::Room;
use uuid::Uuid;

pub fn handle(server: &mut Server, name: &str) -> Result<String, Error> {
    let id = Uuid::new_v4().to_string();
    server.rooms.insert(id.clone(), Room::new(&name));
    server.tx().send(ServerEvent::Updated);
    Ok(serde_json::to_string(&CreateRoomResult{
        id: id
    }).unwrap())
}