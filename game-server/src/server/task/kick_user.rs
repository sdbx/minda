use server::Server;
use error::Error;
use model::UserId;

pub fn handle(server: &mut Server, user_id: UserId, room_id: &str) -> Result<String, Error> {
    let conn_id = {
        let room = server.rooms.get(room_id)?;
        let user = room.get_user(user_id)?;
        user.conn_id.clone()
    };
    server.kick(conn_id);
    Ok("{}".to_owned())
}