use error::Error;
use server::room::Room;
use server::{Server,Connection};
use model::Event::Chated;

pub fn handle(server: &mut Server, conn: &Connection, content: &str) -> Result<(), Error> {
    let room_id = {
        server.get_room(&conn)?.id.clone()
    };
    server.broadcast(&room_id, &Chated {
        user: conn.user_id,
        content: content.to_owned()
    });
    Ok(())
}