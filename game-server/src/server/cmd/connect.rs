use game::Player::Black;
use model::Event;
use server::{Server, Connection};
use error::Error;

pub fn handle(server: &mut Server, conn: &Connection, key: String) -> Result<(), Error> {
    let (room, invite, gameevent) = {
        let invite = match server.invites.get(&key) {
            Some(invite) => invite,
            None => { return Err(Error::InvalidCommand) }
        };

        let room = match server.rooms.get_mut(&invite.room) {
            Some(room) => room,
            None => { return Err(Error::Internal) }
        };

        let conn2 = server.conns.get_mut(&conn.conn_id).unwrap();
        conn2.id = Some(key.clone());
        conn2.room = Some(invite.room.clone());
        room.add_user(&key, conn.conn_id, invite.user);

        if let Some(ref game) = room.game {
            (room.to_model(), invite.clone(), Some(Event::Started{
                black: room.conf.black,
                white: room.conf.white,
                board: game.board.raw().clone(),
                turn: { if game.turn == Black { "black".to_owned() } else { "white".to_owned() } }
            }))
        } else {
            (room.to_model(), invite.clone(), None)
        }
    };

    if let Some(event) = gameevent {
        server.dispatch(conn.conn_id, &event);
    }
    server.broadcast(&invite.room, &Event::Entered{
        user: invite.user
    });
    server.dispatch(conn.conn_id, &Event::Connected{ room: room });
    Ok(())
}