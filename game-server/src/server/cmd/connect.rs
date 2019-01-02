use game::Player::Black;
use model::Event;
use server::{Server, Connection, ServerEvent::Updated};
use error::Error;

pub fn handle(server: &mut Server, conn: &Connection, key: &str) -> Result<(), Error> {
    let (room, invite, gameevent) = {
        let invite = match server.invites.get(key) {
            Some(invite) => invite,
            None => { return Err(Error::InvalidCommand) }
        };

        let room = match server.rooms.get_mut(&invite.room_id) {
            Some(room) => room,
            None => { return Err(Error::Internal) }
        };
        
        if room.users.len() == 0 && invite.user_id != room.conf.king {
            return Err(Error::Permission)
        }

        let conn2 = server.conns.get_mut(&conn.conn_id).unwrap();
        conn2.user_id = invite.user_id;
        conn2.room_id = Some(invite.room_id.clone());
        let mroom = room.to_model();
        room.add_user(conn.conn_id, invite.user_id, &key);

        if let Some(ref game) = room.game {
            (mroom, invite.clone(), Some(Event::Started{
                black: room.conf.black,
                white: room.conf.white,
                board: game.board.raw().clone(),
                turn: { if game.turn == Black { "black".to_owned() } else { "white".to_owned() } }
            }))
        } else {
            (mroom, invite.clone(), None)
        }
    };

    server.invites.remove(&invite.key);
    server.update_discover();
    server.dispatch(conn.conn_id, &Event::Connected{ room: room });
    if let Some(event) = gameevent {
        server.dispatch(conn.conn_id, &event);
    }
    server.broadcast(&invite.room_id, &Event::Entered{
        user: invite.user_id
    });
    Ok(())
}