pub enum ApiEvent {
    Connect { id: String },
    Disconnect { id: String }
}

pub struct ApiEventSend {
    pub api_kind: &'static str,
    pub ev: ApiEvent
}