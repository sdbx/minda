use std::collections::HashMap;
use server::gateway::{GatewayEvent, GatewayEventSend,  GatewayUserManager};
use std::net::{TcpListener, TcpStream};
use std::sync::{Arc, Mutex};
use std::sync::mpsc::{Sender};
use uuid::Uuid;

pub struct SocketListener {
    addr: String,
    event_tx: Sender<GatewayEventSend>,
    user_manager: Arc<Mutex<GatewayUserManager>>,
    streams: Arc<Mutex<HashMap<String, TcpStream>>>
}

impl SocketListener {
    pub fn run(&self) {
        let listener = TcpListener::bind(&self.addr).unwrap();
        for stream in listener.incoming() {
            let stream = stream.unwrap();
            let id = Uuid::new_v4().to_string();
            self.streams.lock().unwrap().insert(id.clone(), stream);
            self.send_ev(GatewayEvent::Connect{id: id});
        }
    }

    fn send_ev(&self, ev: GatewayEvent) {
        self.event_tx.send(GatewayEventSend{
            kind: "socket",
            ev
        });
    }
}
