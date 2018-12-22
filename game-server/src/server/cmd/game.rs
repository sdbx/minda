use model::AxialCord;
use model::Event;
use server::server::Connection;
use server::server::Server;
use error::Error;

pub fn game_move(server: &mut Server, conn: &Connection, start: AxialCord, end: AxialCord, dir: AxialCord) -> Result<(), Error> {
    let id = match conn.id {
        Some(ref id) => id,
        None => { return Err(Error::InvalidCommand) }
    };

    let (room, event) = {
        let name = match conn.room {
            Some(ref x) => x,
            None => { return Err(Error::Internal) }
        };

        let room = match server.rooms.get_mut(name) {
            Some(x) => x,
            None => { return Err(Error::Internal) }
        };

        let game = match room.game {
            Some(ref mut x) => x,
            None => { return Err(Error::InvalidCommand) }
        };

        let turn = match game.get_turn(&id) {
            Ok(x) => x,
            Err(_) => { return Err(Error::InvalidCommand) }
        };

        game.run_move(id, start, end, dir)?;
        (name.clone(), Event::Moved {
            player: turn.to_string(),
            start: start,
            end: end,
            dir: dir
        })
    };

    server.broadcast(&room, &event);
    Ok(())
}
