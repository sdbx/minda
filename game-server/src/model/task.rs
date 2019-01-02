use super::{RoomConf, UserId};

#[derive(Serialize, Deserialize, Debug)]
pub struct TaskRequest {
    pub id: String,
    pub task: Task
}

#[derive(Clone, Serialize, Deserialize, Debug)]
#[serde(tag = "kind")]
pub enum Task {
    #[serde(rename = "create-room")]
    CreateRoom { conf: RoomConf, user_id: UserId },
    #[serde(rename = "join-room")]
    JoinRoom { room_id: String, user_id: UserId },
    #[serde(rename = "kick-user")]
    KickUser { room_id: String, user_id: UserId }
}

#[derive(Serialize, Deserialize, Debug)]
pub struct TaskResult {
    pub error: Option<String>,
    pub value: String
}

#[derive(Serialize, Deserialize, Debug)]
pub struct CreateRoomResult {
    pub invite: String,
    pub addr: String
}

#[derive(Serialize, Deserialize, Debug)]
pub struct JoinRoomResult {
    pub invite: String,
    pub addr: String
}
