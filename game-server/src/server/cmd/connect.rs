use game::Player::Black;
use server::cmd;
use server::room::Room;
use model::Event;
use server::{Server, Connection};
use error::Error;
use uuid::Uuid;

pub fn handle(server: &mut Server, conn: &Connection, id: String) -> Result<(), Error> {
    let (roomname, invite, gameevent) = {
        let invite = match server.invites.get(&id) {
            Some(invite) => invite,
            None => { return Err(Error::InvalidCommand) }
        };

        let room = match server.rooms.get_mut(&invite.room) {
            Some(room) => room,
            None => { return Err(Error::Internal) }
        };

        let conn2 = server.conns.get_mut(&conn.conn_id).unwrap();
        conn2.id = Some(id.clone());
        conn2.room = Some(invite.room.clone());
        room.add_user(&id, conn.conn_id, invite.user.clone());

        if let Some(ref game) = room.game {
            (room.name.clone(), invite.clone(), Some(Event::GameStart{
                black: room.get_username_or_empty(&game.black),
                white: room.get_username_or_empty(&game.white),
                board: game.board.raw().clone(),
                turn: { if game.turn == Black { "black".to_owned() } else { "white".to_owned() } }
            }))
        } else {
            (room.name.clone(), invite.clone(), None)
        }
    };

    if let Some(event) = gameevent {
        server.dispatch(conn.conn_id, &event);
    }
    server.broadcast(&invite.room, &Event::Enter{
        username: invite.user.username
    });
    server.dispatch(conn.conn_id, &Event::Connected{ roomname: roomname });
    Ok(())
}