use server::{Server, Connection};
use model::{RoomConf,UserId};
use model::Event::Confed;
use error::Error;

pub fn handle(server: &mut Server, conn: &Connection, conf: &RoomConf) -> Result<(), Error> {
    let room = {
        let room = server.get_room_mut(&conn)?;
        if room.conf.king != conn.user_id {
            return Err(Error::Permission)
        }
        if (!room.exists_user(conf.black) && conf.black != UserId::empty) || 
            (!room.exists_user(conf.white) && conf.white != UserId::empty) || 
            !room.exists_user(conf.king) {
            return Err(Error::InvalidParm)
       }
       room.conf = conf.clone();
       room.id.clone()
    };
    server.broadcast(&room, &Confed{
        conf: conf.clone(),
    });
    Ok(())
}
