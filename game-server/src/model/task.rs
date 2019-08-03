use game::Move;
use model::EndedCause;
use model::GameRule;
use super::{RoomConf, RoomRank, UserId};

#[derive(Serialize, Deserialize, Debug)]
pub struct TaskRequest {
    pub id: String,
    pub task: Task
}

#[derive(Clone, Serialize, Deserialize, Debug)]
#[serde(tag = "kind")]
pub enum Task {
    #[serde(rename = "create-room")]
    CreateRoom { room_id: String, conf: RoomConf, user_id: UserId, rank: Option<RoomRank> },
    #[serde(rename = "join-room")]
    JoinRoom { room_id: String, user_id: UserId },
    #[serde(rename = "kick-user")]
    KickUser { room_id: String, user_id: UserId },
    #[serde(rename = "delete-room")]
    DeleteRoom { room_id: String },
    #[serde(rename = "complete-game")]
    CompleteGame { black: UserId, rank: bool, white: UserId, loser: String, cause: EndedCause, map: String, game_rule: GameRule, moves: Vec<Move>},
}

#[derive(Serialize, Deserialize, Debug)]
pub struct TaskResult {
    pub error: Option<String>,
    pub value: String
}

#[derive(Serialize, Deserialize, Debug)]
pub struct LobbyRoomResult {
    pub invite: String,
    pub addr: String
}

#[derive(Serialize, Deserialize, Debug)]
pub struct CompleteGameResult {
    pub winner_delta: f64,
    pub loser_delta: f64 
}