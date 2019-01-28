use model::LobbyRoomResult;
use model::{UserId, RoomConf};
use server::{Server, ServerEvent};
use model::{Invite};
use error::Error;
use server::room::Room;

pub fn handle(server: &mut Server, room_id: &str, user_id: UserId, conf: &RoomConf) -> Result<String, Error> {
    let room = Room::new(room_id, &conf);
    let invite = Invite::new(user_id, &room.id);
    let res = LobbyRoomResult{
        invite: invite.key.clone(),
        addr: server.real_addr.clone()
    };
    server.rooms.insert(room.id.clone(), room);
    server.invites.insert(invite.key.clone(), invite);
    server.update_discover();
    Ok(serde_json::to_string(&res).unwrap())
}