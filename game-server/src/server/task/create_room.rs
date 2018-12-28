use model::CreateRoomResult;
use model::{UserId, RoomConf};
use server::{Server, ServerEvent};
use model::{Invite};
use error::Error;
use server::room::Room;

pub fn handle(server: &mut Server, user: UserId, conf: &RoomConf) -> Result<String, Error> {
    let room = Room::new(&conf);
    let invite = Invite::new(user, &room.id);
    let res = CreateRoomResult{
        invite: invite.key.clone(),
        addr: server.real_addr.clone()
    };
    server.rooms.insert(room.id.clone(), room);
    server.invites.insert(invite.key.clone(), invite);
    server.tx().send(ServerEvent::Updated);
    Ok(serde_json::to_string(&res).unwrap())
}