use std::collections::HashSet;
use model::EventSend;
use server::gateway::{Gateway, GatewayUserManager, GatewayEventHandler, GatewayEventSend, GatewayEvent};
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use std::sync::mpsc;
use std::sync::mpsc::{Receiver, Sender};
use std::thread;

pub struct GatewayManager {
    event_tx: Sender<GatewayEventSend>,
    user_manager: Arc<Mutex<GatewayUserManager>>,
    gateways: HashMap<&'static str, Box<Gateway>>
}

impl GatewayManager {
    pub fn new() -> Arc<Mutex<Self>> {
        let (event_tx, event_rx) = mpsc::channel();
        let gateway_event_handler = GatewayEventHandler::new(event_rx);
        thread::spawn(move || {
            gateway_event_handler.run();
        });

        Arc::new(Mutex::new(Self {
            event_tx: event_tx,
            user_manager: GatewayUserManager::new(),
            gateways: HashMap::new()
        }))
    }

    pub fn add(&mut self, gateway: Box<Gateway>) {
        self.gateways.insert(gateway.kind(), gateway);
    }

    pub fn run(&mut self) {
        for (_, gate) in self.gateways.iter() {
            gate.run(self.event_tx.clone(), self.user_manager.clone());
        }
    }

    pub fn dispatch(&self, ev: EventSend) {
        if let Some(user) = self.user_manager.lock().unwrap().get(&ev.user_id) {
            let gate = self.gateways.get(user.kind).unwrap();
            gate.dispatch(&ev.user_id, ev.ev);
        }
    }
}