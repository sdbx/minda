use board::Cord;
use board::Stone;

#[derive(Deserialize, Serialize, Copy, Clone, PartialEq, Debug)]
pub struct AxialCord {
    pub x: isize,
    pub y: isize
}

impl AxialCord {
    pub fn to_cord(&self) -> Cord {
        Cord(self.x, self.y, -self.x-self.y)
    }
}

#[derive(Clone, Serialize, Deserialize, Debug)]
#[serde(tag = "type")]
pub enum Event {
    #[serde(rename = "connected")]
    Connected { roomname: String },
    #[serde(rename = "gamestart")]
    GameStart{ board: Vec<Vec<Stone>>, black: String, white: String, turn: String },
    #[serde(rename = "enter")]
    Enter { username: String },
    #[serde(rename = "110vbabu")]
    Board { board: Vec<Vec<Stone>> },
    #[serde(rename = "error")]
    Error { message: String },
    #[serde(rename = "move")]
    Move { player: String, start: AxialCord, end: AxialCord, dir: AxialCord },
}

#[derive(Clone, Debug)]
pub struct Invite {
    pub id: String,
    pub user: User,
    pub room: String
}

#[derive(Serialize, Deserialize, Debug)]
#[serde(tag = "type")]
pub enum Command {
    #[serde(rename = "connect")]
    Connect { id: String },
    #[serde(rename = "move")]
    Move { start: AxialCord, end: AxialCord, dir: AxialCord }
}

pub fn parse_command(msg: &str) -> Result<Command, serde_json::Error> {
    serde_json::from_str(msg)
}

#[derive(Clone, Debug)]
pub struct User {
    pub username: String
}