use model::{Invite};
use error::Error;
use server::Server;
use server::room::Room;

pub fn handle(server: &mut Server, room_id: &str) -> Result<String, Error> {
    {
        let room = server.rooms.get(room_id)?;
        if room.users.len() != 0 {
            return Err(Error::RoomNotEmpty)
        }
    }
    server.delete_room(&room_id);
    println!("[room:{}] deleted", room_id);
    Ok("{}".to_owned())
}