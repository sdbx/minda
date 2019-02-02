use uuid::Uuid;
use error::Error;
use model::Event;
use server::room::Room;
use server::{Server,Connection};
use model::Event::Chated;

pub fn chat(server: &mut Server, conn: &Connection, content: &str) -> Result<(), Error> {
    let room_id = server.get_room(&conn)?.id.clone();
    server.broadcast(&room_id, &Chated {
        user: conn.user_id,
        content: content.to_owned()
    });
    Ok(())
}

pub fn connect(server: &mut Server, conn: &Connection, key: &str) -> Result<(), Error> {
    let (room, invite, gameevent, kick_users) = {
        let invite = match server.invites.get(key) {
            Some(invite) => invite,
            None => { return Err(Error::InvalidParm) }
        };

        let room = match server.rooms.get_mut(&invite.room_id) {
            Some(room) => room,
            None => { return Err(Error::Internal) }
        };
        
        // empty room should be meaningless until the king comes.
        if room.users.len() == 0 && invite.user_id != room.conf.king {
            return Err(Error::Permission)
        }
        if room.banned_users.contains(&invite.user_id) {
            return Err(Error::Banned)
        } 

        let kick_users: Vec<Uuid> = room.get_users(invite.user_id).iter().map(|u| u.conn_id).collect();

        let conn = server.conns.get_mut(&conn.conn_id).unwrap();
        conn.user_id = invite.user_id;
        conn.room_id = Some(invite.room_id.clone());

        let mroom = room.to_model();
        room.add_user(conn.conn_id, invite.user_id, &key);
        if let Some(ref game) = room.game {
            let ticked = Some(Event::Ticked {
                white_time: (game.white_time as f32) / 1000.0,
                black_time: (game.black_time as f32) / 1000.0,
                current_time: (game.current_time as f32) / 1000.0
            });
            (mroom, invite.clone(), Some(Event::game_to_started(game)), kick_users)
        } else {
            (mroom, invite.clone(), None, kick_users)
        }
    };

    server.invites.remove(&invite.key);
    server.update_discover()?;

    server.dispatch(conn.conn_id, &Event::Connected{ room: room });

    if let Some(event) = gameevent {
        server.dispatch(conn.conn_id, &event);
    }
    server.broadcast(&invite.room_id, &Event::Entered{
        user: invite.user_id
    });
    kick_users.iter().for_each(|u| server.kick(*u));
    Ok(())
}