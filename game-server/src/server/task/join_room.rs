use model::LobbyRoomResult;
use model::{UserId, RoomConf};
use server::{Server, ServerEvent};
use model::{Invite};
use error::Error;
use server::room::Room;

pub fn handle(server: &mut Server, user_id: UserId, room_id: &str) -> Result<String, Error> {
    server.rooms.get(room_id)?;
    let key;
    if let Some(invite) = server.get_invite_of_user(user_id, room_id) {
        key = invite.key;
    } else {
        let invite = Invite::new(user_id, &room_id);
        key = invite.key.clone();
        server.invites.insert(invite.key.clone(), invite);
    }
    let res = LobbyRoomResult{
        invite: key,
        addr: server.real_addr.clone()
    };
    Ok(serde_json::to_string(&res).unwrap())
}