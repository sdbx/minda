use model::Event::Confed;
use model::RoomConf;
use server::{Server,Connection};
use game::Game;
use game::Board;
use model::{UserId, Event};
use error::Error;

pub fn start(server: &mut Server, conn: &Connection) -> Result<(), Error> { 
    let (room_id, event) = {
        let room = server.get_room_mut(&conn)?;
        if room.conf.king != conn.user_id {
            return Err(Error::Permission)
        }
        if room.conf.black == UserId::empty || room.conf.white == UserId::empty {
            return Err(Error::InvalidState)
        }
        if !room.game.is_none() {
            return Err(Error::GameStarted)
        }
        room.start()?;
        let event = Event::game_to_started(room.game.as_ref().unwrap());
        (room.id.clone(), event)
    };
    server.broadcast(&room_id, &event);
    Ok(())
}

pub fn conf(server: &mut Server, conn: &Connection, conf: &RoomConf) -> Result<(), Error> {
    let room = {
        let room = server.get_room_mut(&conn)?;
        if room.conf.king != conn.user_id {
            return Err(Error::Permission)
        }
        room.set_conf(&conf)?;
        room.id.clone()
    };
    server.broadcast(&room, &Confed{
        conf: conf.clone(),
    });
    Ok(())
}

pub fn ban(server: &mut Server, conn: &Connection, user: UserId) -> Result<(), Error> {
    let (event, conn_id) = {
        let room = server.get_room_mut(&conn)?;
        if room.conf.king != conn.user_id {
            return Err(Error::Permission)
        }
        if !room.exists_user(user) {
            return Err(Error::InvalidParm)
        }
        room.banned_users.insert(user);
        let room_user = room.get_user(user).unwrap();
        (Event::Banned {
            user: user,
        }, room_user.conn_id)
    };
    server.kick(conn_id);
    server.broadcast(conn.room_id.as_ref().unwrap(), &event);
    Ok(())
}