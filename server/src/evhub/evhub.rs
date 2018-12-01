use game::Stone;
use std::sync::mpsc::{Sender, Receiver};

#[derive(Serialize, Deserialize, Debug)]
#[serde(tag = "type")]
pub enum Event {
    #[serde(rename = "id")]
    Id { id: String },
    #[serde(rename = "board")]
    Board { board: Vec<Vec<Stone>> },
    #[serde(rename = "error")]
    Error { message: String },
}

pub struct EventSend {
    pub to: String,
    pub ev: Event
}

pub enum ServerEvent {
    Connect { id: String }
}

pub struct ServerEventSend {
    pub from: &'static str,
    pub ev: ServerEvent
}
