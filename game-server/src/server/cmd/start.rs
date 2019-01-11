use server::{Server,Connection};
use game::Game;
use game::Board;
use model::{UserId, Event};
use error::Error;

pub fn handle(server: &mut Server, conn: &Connection) -> Result<(), Error> { 
    let (room_id, event) = {
        let room = server.get_room_mut(&conn)?;
        if room.conf.king != conn.user_id {
            return Err(Error::Permission)
        }
        if room.conf.black == UserId::empty || room.conf.white == UserId::empty {
            return Err(Error::InvalidParm)
        }
        if !room.game.is_none() {
            return Err(Error::GameStarted)
        }
        let game = Game::new(room.conf.black, room.conf.white, Board::from_string("0@0@0@0@0@0@0@2@2#0@0@0@0@0@0@0@2@2#0@0@0@0@0@0@2@2@2#0@1@0@0@0@0@2@2@2#1@1@1@0@0@0@2@2@2#1@1@1@0@0@0@0@2@0#1@1@1@0@0@0@0@0@0#1@1@0@0@0@0@0@0@0#1@1@0@0@0@0@0@0@0").unwrap());
        let event = Event::game_to_started(&game);
        room.game = Some(game);
        (room.id.clone(), event)
    };
    server.broadcast(&room_id, &event);
    Ok(())
}