use model::RoomConf;
use super::{UserId, AxialCord, Room};
use game::{Stone, Game};
use server::Server;
use chrono::Utc;
use chrono::DateTime;
use uuid::Uuid;

#[derive(Clone, Debug)]
pub struct Invite {
    pub key: String,
    pub user_id: UserId,
    pub room_id: String
}

impl Invite {
    pub fn new(user_id: UserId, room_id: &str) -> Self {
        Self {
            key: Uuid::new_v4().to_string(),
            user_id: user_id.clone(),
            room_id: room_id.to_owned()
        }
    }
}

#[derive(Clone, Serialize, Deserialize, Debug)]
#[serde(tag = "type")]
pub enum Event {
    #[serde(rename = "connected")]
    Connected { room: Room },
    #[serde(rename = "started")]
    Started { board: Vec<Vec<Stone>>, black: UserId, white: UserId, turn: String },
    #[serde(rename = "entered")]
    Entered { user: UserId },
    #[serde(rename = "error")]
    Error { message: String },
    #[serde(rename = "moved")]
    Moved { player: String, start: AxialCord, end: AxialCord, dir: AxialCord },
    #[serde(rename = "chated")]
    Chated { user: UserId, content: String },
    #[serde(rename = "confed")]
    Confed { conf: RoomConf },
    #[serde(rename = "left")]
    Left { user: UserId }
}

impl Event {
    pub fn game_to_started(game: &Game) -> Self {
        Event::Started {
            board: game.board.raw(),
            black: game.black,
            white: game.white,
            turn: game.turn.to_string()
        }
    }
}

#[derive(Serialize, Deserialize, Debug)]
#[serde(tag = "type")]
pub enum Command {
    #[serde(rename = "connect")]
    Connect { invite: String },
    #[serde(rename = "move")]
    Move { start: AxialCord, end: AxialCord, dir: AxialCord },
    #[serde(rename = "chat")]
    Chat { content: String },
    #[serde(rename = "conf")]
    Conf { conf: RoomConf },
    #[serde(rename = "start")]
    Start { },
}

pub fn parse_command(msg: &str) -> Result<Command, serde_json::Error> {
    serde_json::from_str(msg)
}

#[derive(Serialize, Deserialize, Debug)]
pub struct GameServer {
    pub name: String,
    pub addr: String,
    pub rooms: Vec<Room>,
    pub last_ping: DateTime<Utc>
}

impl GameServer {
    pub fn from_server(server: &Server) -> Self {
        let rooms = server.rooms.iter().map(|(_, room)| {
            room.to_model()
        }).collect::<Vec<_>>();
        Self {
            name: server.name.clone(),
            addr: server.real_addr.clone(),
            rooms: rooms,
            last_ping: Utc::now()
        }
    }
}