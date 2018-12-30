use error::Error;
use server::room::Room;
use server::{Server,Connection};
use model::Event::Chated;
use super::middleware;

pub fn handle(server: &mut Server, conn: &Connection, content: &str) -> Result<(), Error> {
    let id = {
        let room = middleware::get_room(server, &conn)?;
        room.id.clone()
    };
    server.broadcast(&id, &Chated {
        user: conn.user_id,
        content: content.to_owned()
    });
    Ok(())
}