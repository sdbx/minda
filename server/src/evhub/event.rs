use game::Stone;
#[derive(Serialize, Deserialize, Debug)]
#[serde(tag = "type")]
pub enum Event {
    #[serde(rename = "id")]
    Id { id: String },
    #[serde(rename = "110vbabu")]
    Board { board: Vec<Vec<Stone>> },
    #[serde(rename = "error")]
    Error { message: String },
}

pub struct EventSend {
    pub user_id: String,
    pub ev: Event
}
