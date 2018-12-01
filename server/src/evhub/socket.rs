use std::collections::HashMap;
use evhub::{Event, EventSend, ServerEvent, ServerEventSend};
use std::io::Write;
use std::net::{TcpListener, TcpStream};
use std::sync::{Arc, Mutex};
use std::sync::mpsc::{Sender, Receiver};
use std::sync::mpsc;
use std::thread;
use uuid::Uuid;

pub struct SocketHub {
    addr: String,
    send: Sender<ServerEventSend>,
    streams: HashMap<String, TcpStream>
}

impl SocketHub {
    pub const name: &'static str = "socket_hub";

    pub fn start(addr: &str, send: Sender<ServerEventSend>) -> Sender<EventSend> {
        let (out, recv): (Sender<EventSend>, Receiver<EventSend>)= mpsc::channel();
        let hub = Arc::new(Mutex::new(SocketHub::new(addr, send)));

        let h = hub.clone();
        thread::spawn(move || {
            loop {
                if let Ok(es) = recv.recv() {
                    h.lock().unwrap().dispatch(&es.to, es.ev);
                }
            }
        });

        let listener = TcpListener::bind(&hub.lock().unwrap().addr).unwrap();
        let h = hub.clone();
        thread::spawn(move || {
            for stream in listener.incoming() {
                let stream = stream.unwrap();
                println!("asdfsadf");
                h.lock().unwrap().add_stream(stream);
                println!("asdfsadf");
            }
        });

        out
    }

    fn new(addr: &str, send: Sender<ServerEventSend>) -> Self {
        Self {
            send: send,
            addr: addr.to_owned(),
            streams: HashMap::new(),
        }
    }

    fn add_stream(&mut self, stream: TcpStream) {
        let id = Uuid::new_v4().to_string();
        self.streams.insert(id.clone(), stream);
        self.dispatch(&id, Event::Id{id: id.clone()});
        self.send_event(ServerEvent::Connect{id: id});
    }

    fn send_event(&self, ev: ServerEvent) {
        self.send.send(ServerEventSend{ from: SocketHub::name, ev: ev });
    }

    fn dispatch(&mut self, to: &str, ev: Event) {
        if let Some(stream) = self.streams.get_mut(to) {
            let st = serde_json::to_string(&ev).unwrap();
            println!("asdfasdf");
            println!("{}", st);
            stream.write(st.as_bytes());
        }
    }
}