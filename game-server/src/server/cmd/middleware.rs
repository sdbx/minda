use error::Error;
use server::room::Room;
use server::{Server,Connection};

pub fn get_room<'a>(server: &'a mut Server, conn: &Connection) -> Result<&'a mut Room, Error> {
    let name = match conn.room {
        Some(ref x) => x,
        None => { return Err(Error::Internal) }
    };

    let room = match server.rooms.get_mut(name) {
        Some(x) => x,
        None => { return Err(Error::Internal) }
    };

    Ok(room)
}