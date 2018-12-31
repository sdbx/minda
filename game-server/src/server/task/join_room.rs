use model::JoinRoomResult;
use model::{UserId, RoomConf};
use server::{Server, ServerEvent};
use model::{Invite};
use error::Error;
use server::room::Room;

pub fn handle(server: &mut Server, user: UserId, room_id: &str) -> Result<String, Error> {
    server.rooms.get(room_id)?;
    let invite = Invite::new(user, &room_id);
    let res = JoinRoomResult{
        invite: invite.key.clone(),
        addr: server.real_addr.clone()
    };
    server.invites.insert(invite.key.clone(), invite);
    Ok(serde_json::to_string(&res).unwrap())
}