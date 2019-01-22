use model::AxialCord;
use model::Event;
use server::server::Connection;
use server::server::Server;
use error::Error;

pub fn game_move(server: &mut Server, conn: &Connection, start: AxialCord, end: AxialCord, dir: AxialCord) -> Result<(), Error> {
    let (room, event) = {
        let room = server.get_room(&conn)?;
        let game = server.get_game_mut(&conn)?;
        let turn = match game.get_turn(conn.user_id) {
            Ok(x) => x,
            Err(_) => { return Err(Error::InvalidState) }
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

pub fn game_gg(server: &mut Server, conn: &Connection) -> Result<(), Error> {

}