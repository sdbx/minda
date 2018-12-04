use api::ApiEventSend;
use api::ApiEvent;
use std::sync::mpsc::Receiver;

pub struct ApiEventHandler {
    event_rx: Receiver<ApiEventSend>
}

impl ApiEventHandler {
    pub fn new(event_rx: Receiver<ApiEventSend>) -> Self{
        Self {
            event_rx: event_rx
        }
    }

    pub fn run(&self) {
        while let Ok(ev) = self.event_rx.recv() {
            match ev.ev {
                ApiEvent::Connect{id: id} => {
                },
                _ => {}
            }
        }
    }
}