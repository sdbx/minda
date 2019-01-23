use model::AxialCord;
use model::Event;
use model::EndedCause;
use game::Player;
use server::server::Connection;
use server::server::Server;
use error::Error;

pub fn game_move(server: &mut Server, conn: &Connection, start: AxialCord, end: AxialCord, dir: AxialCord) -> Result<(), Error> {
    let event = {
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
        Event::Moved {
            player: turn.to_string(),
            start: start,
            end: end,
            dir: dir
        }
    };

    server.broadcast(conn.room_id.as_ref().unwrap(), &event);
    Ok(())
}

pub fn game_gg(server: &mut Server, conn: &Connection) -> Result<(), Error> {
    let event = {
        let room = server.get_room_mut(&conn)?;
        let game = match room.game.as_ref() {
            Some(x) => x,
            None => return Err(Error::InvalidState)
        };
        if game.black == conn.user_id || game.white == conn.user_id {
            if game.black == conn.user_id {
                Event::Ended {
                    loser: game.black,
                    color: Player::Black.to_string(),
                    cause: EndedCause::Gg
                }
            } else {
                Event::Ended {
                    loser: game.white,
                    color: Player::White.to_string(),
                    cause: EndedCause::Gg
                }
            }
        } else {
            return Err(Error::Permission)
        }
    };
    {
        let room = server.get_room_mut(&conn)?;
        room.game = None;
    }
    server.broadcast(conn.room_id.as_ref().unwrap(), &event);
    server.update_discover();
    Ok(())
}