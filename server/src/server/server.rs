use model::EventSend;
use super::gateway::{Gateway, GatewayManager, GatewayEventHandler};
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use std::thread;

pub struct Server {
    gateway_manager: Arc<Mutex<GatewayManager>>
}

impl Server {
    pub fn new() -> Self {
        Self {
            gateway_manager: GatewayManager::new(),
        }
    }

    pub fn add_gateway(&self, gateway: Box<Gateway>) {
        self.gateway_manager.lock().unwrap().add(gateway);
    }

    pub fn run(&self) {
        self.gateway_manager.lock().unwrap().run();

    }
}