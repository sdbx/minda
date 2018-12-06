use super::{GatewayEvent, GatewayEventSend};
use std::sync::mpsc::Receiver;

pub struct GatewayEventHandler {
    event_rx: Receiver<GatewayEventSend>
}

impl GatewayEventHandler {
    pub fn new(event_rx: Receiver<GatewayEventSend>) -> Self{
        Self {
            event_rx: event_rx
        }
    }

    pub fn run(&self) {
        while let Ok(ev) = self.event_rx.recv() {
            match ev.ev {
                GatewayEvent::Connect{id: id} => {
                },
                _ => {}
            }
        }
    }
}