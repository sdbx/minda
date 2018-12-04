use api::socket::socket_listener::SocketListener;
use std::collections::HashMap;
use api::{Api, ApiEventSend};
use evhub::Event;
use std::io::Write;
use std::net::{TcpStream};
use std::sync::{Arc, Mutex};
use std::sync::mpsc::{Sender};
use std::thread;

pub struct SocketApi {
    addr: String,
    streams: Arc<Mutex<HashMap<String, TcpStream>>>
}

impl SocketApi {
    pub fn new(addr: &str) -> SocketApi {
        Self {
            addr: addr.to_owned(),
            streams: Arc::new(Mutex::new(HashMap::new()))
        }
    }
}

impl Api for SocketApi {
    fn run(&self, event_tx: Sender<ApiEventSend>) {
        let socket_listener = SocketListener {
            addr: self.addr,
            event_tx: event_tx,
            streams: self.streams.clone()
        };
        thread::spawn(move || {
            socket_listener.run();
        });
    }

    fn dispatch(&self, to: &str, ev: Event) {
        if let Some(stream) = self.streams.lock().unwrap().get_mut(to) {
            let st = serde_json::to_string(&ev).unwrap();
            println!("{}", st);
            stream.write(st.as_bytes());
        }
    }

    fn kind(&self) -> &'static str {
        "socket_hub"
    }
}