use model::LobbyRoomResult;
use model::{UserId, RoomConf, RoomRank};
use server::{Server, ServerEvent};
use model::{Invite};
use error::Error;
use server::room::Room;

pub fn handle(server: &mut Server, room_id: &str, user_id: UserId, conf: &RoomConf, rank: Option<&RoomRank>) -> Result<String, Error> {
    let room = Room::new(room_id, &conf, rank);
    let res = if rank.is_some() {
        server.rooms.insert(room.id.clone(), room);
        server.update_discover();
        LobbyRoomResult{
            invite: "".to_owned(),
            addr: "".to_owned()
        }
    } else {
        let invite = Invite::new(user_id, &room.id);
        server.rooms.insert(room.id.clone(), room);
        server.invites.insert(invite.key.clone(), invite.clone());
        server.update_discover();
        LobbyRoomResult{
            invite: invite.key,
            addr: server.real_addr.clone()
        }
    };
    if rank.is_some() {
        println!("[room:{}] created by user({:?})", room_id, user_id);
    } else {
        println!("[room:{}] created by user({:?}) as ranked", room_id, user_id);
    }
    
    Ok(serde_json::to_string(&res).unwrap())
}