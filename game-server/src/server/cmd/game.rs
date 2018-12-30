use model::AxialCord;
use model::Event;
use server::server::Connection;
use server::server::Server;
use error::Error;

pub fn game_move(server: &mut Server, conn: &Connection, start: AxialCord, end: AxialCord, dir: AxialCord) -> Result<(), Error> {
    let (room, event) = {
        let room = server.get_room_mut(&conn)?;

        let game = match room.game {
            Some(ref mut x) => x,
            None => { return Err(Error::InvalidCommand) }
        };

        let turn = match game.get_turn(conn.user_id) {
            Ok(x) => x,
            Err(_) => { return Err(Error::InvalidCommand) }
        };

        game.run_move(conn.user_id, start, end, dir)?;
        (room.id.clone(), Event::Moved {
            player: turn.to_string(),
            start: start,
            end: end,
            dir: dir
        })
    };

    server.broadcast(&room, &event);
    Ok(())
}
