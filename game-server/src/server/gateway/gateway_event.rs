pub enum GatewayEvent {
    Connect { id: String },
    Disconnect { id: String }
}

pub struct GatewayEventSend {
    pub kind: &'static str,
    pub ev: GatewayEvent
}