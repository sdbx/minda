use model::AxialCord;
use model::Event;
use server::server::Connection;
use server::server::Server;

pub fn game_move(server: &mut Server, conn: &Connection, start: AxialCord, end: AxialCord, dir: AxialCord) {
    let id = match conn.id {
        Some(ref id) => id,
        None => { return }
    };

    let (room, event) = {
        let name = match conn.room {
            Some(ref x) => x,
            None => { return }
        };

        let room = match server.rooms.get_mut(name) {
            Some(x) => x,
            None => { return }
        };

        let game = match room.game {
            Some(ref mut x) => x,
            None => { return }
        };

        let turn = match game.get_turn(&id) {
            Ok(x) => x,
            Err(_) => { return }
        };

        match game.run_move(id, start, end, dir) {
            Ok(_) =>  (name.clone(), Event::Move {
                player: turn.to_string(),
                start: start,
                end: end,
                dir: dir
            }),
            Err(_) => { return }
        }
    };

    server.broadcast(&room, event);
}
