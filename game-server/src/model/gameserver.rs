use model::GameRule;
use serde::Deserialize;
use serde::Deserializer;
use serde::Serializer;
use serde::de::Error;
use serde::Serialize;
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

#[derive(Clone, Debug)]
pub enum EndedCause {
    Timeout,
    Gg,
    LostStones
}

impl Serialize for EndedCause {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
        where S: Serializer
    {
        serializer.serialize_str(match *self {
            EndedCause::Timeout => "timeout",
            EndedCause::Gg => "gg",
            EndedCause::LostStones => "lost stones",
        })
    }
}

impl<'de> Deserialize<'de> for EndedCause {
    fn deserialize<D>(deserializer: D) -> Result<Self, D::Error>
        where D: Deserializer<'de>
    {
        let s = String::deserialize(deserializer)?;
        let out = match s.as_str() {
            "timeout" => EndedCause::Timeout,
            "gg" => EndedCause::Gg,
            "lost stones" => EndedCause::LostStones,
            _ => return Err(D::Error::custom("Invalid ended cause"))
        };
        Ok(out)
    }
}

#[derive(Clone, Serialize, Deserialize, Debug)]
#[serde(tag = "type")]
pub enum Event {
    #[serde(rename = "connected")]
    Connected { room: Room },
    #[serde(rename = "started")]
    Started { map: String, black: UserId, white: UserId, turn: String, rule: GameRule, white_time: f32, black_time: f32, current_time: f32 },
    #[serde(rename = "entered")]
    Entered { user: UserId },
    #[serde(rename = "error")]
    Error { message: String },
    #[serde(rename = "moved")]
    Moved { player: String, start: AxialCord, end: AxialCord, dir: AxialCord, map: String },
    #[serde(rename = "chated")]
    Chated { user: UserId, content: String },
    #[serde(rename = "confed")]
    Confed { conf: RoomConf },
    #[serde(rename = "left")]
    Left { user: UserId },
    #[serde(rename = "ended")]
    Ended { loser: UserId, player: String, cause: EndedCause, winner_delta: f64, loser_delta: f64 },
    #[serde(rename = "banned")]
    Banned { user: UserId },
    #[serde(rename = "ticked")]
    Ticked { black_time: f32, white_time: f32, current_time: f32 },
}

impl Event {
    pub fn game_to_started(game: &Game) -> Self {
        Event::Started {
            map: game.board.to_string(),
            black: game.black,
            white: game.white,
            turn: game.turn.to_string(),
            rule: game.rule.clone(),
            black_time: game.black_time as f32 / 1000.0,
            white_time: game.white_time as f32 / 1000.0,
            current_time: game.current_time as f32 / 1000.0
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
    #[serde(rename = "ban")]
    Ban { user: UserId },
    #[serde(rename = "gg")]
    Gg { }
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