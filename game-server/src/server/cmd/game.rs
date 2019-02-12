use tool::print_err;
use model::AxialCord;
use model::Event;
use model::EndedCause;
use game::Player;
use server::server::Connection;
use server::server::Server;
use error::Error;

pub fn game_move(server: &mut Server, conn: &Connection, start: AxialCord, end: AxialCord, dir: AxialCord) -> Result<(), Error> {
    let (event1, event2, lose) = {
        let room = server.get_room_mut(&conn)?;
        let game_opt = room.game.as_mut();
        let game = match game_opt {
            Some(x) => x,
            None => return Err(Error::InvalidState)
        };
        let turn = match game.get_turn(conn.user_id) {
            Ok(x) => x,
            Err(_) => { return Err(Error::InvalidState) }
        };

        game.run_move(conn.user_id, start, end, dir)?;
        (Event::Moved {
            player: turn.to_string(),
            start: start,
            end: end,
            dir: dir,
            map: game.board.to_string()
        },
        Event::Ticked {
            white_time: (game.white_time as f32) / 1000.0,
            black_time: (game.black_time as f32) / 1000.0,
            current_time: (game.current_time as f32) / 1000.0
        },
        game.get_lose())
    };
    let room_id = conn.room_id.as_ref().unwrap();
    server.broadcast(&room_id, &event1);
    server.broadcast(&room_id, &event2);
    if let Some((loser, cause)) = lose {
        print_err(server.complete_game(&room_id, loser, &cause));
    }
    Ok(())
}

pub fn game_gg(server: &mut Server, conn: &Connection) -> Result<(), Error> {
    let loser = {
        let room = server.get_room(&conn)?;
        let game = match room.game.as_ref() {
            Some(x) => x,
            None => return Err(Error::InvalidState)
        };
        if game.black != conn.user_id && game.white != conn.user_id {
            return Err(Error::Permission)
        }
        if game.black == conn.user_id {
            Player::White
        } else {
            Player::Black
        }
    };
    print_err(server.complete_game(conn.room_id.as_ref().unwrap(), loser, &EndedCause::Gg));
    server.update_discover()?;
    Ok(())
}