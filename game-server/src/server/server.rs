use std::sync::mpsc::Sender;
use std::sync::mpsc::channel;
use std::sync::mpsc::Receiver;
use uuid::Uuid;
use std::net::{TcpStream, TcpListener};
use model::{EventSend, Invite};
use server::room::Room;
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use std::io::{Read, Write};
use std::thread;

pub enum ServerEvent {
    Connect{ conn_id: Uuid, conn: TcpStream },
    Close { conn_id: Uuid }
}

pub struct Server {
    tx: Option<Sender<ServerEvent>>,
    addr: String,
    pub rooms: HashMap<String, Room>,
    pub invites: HashMap<String, Invite>,
    pub conns: HashMap<Uuid, TcpStream>
}

impl Server {
    pub fn new(addr: &str) -> Self {
        Self {
            tx: None,
            addr: addr.to_owned(),
            rooms: HashMap::new(),
            invites: HashMap::new(),
            conns: HashMap::new()
        }
    }

    pub fn serve(mut self) {
        for event in self.listen().iter() {
            match event {
                ServerEvent::Connect{ conn_id: conn_id, conn: conn } => {
                    self.conns.insert(conn_id, conn);
                }
            }
        }
    }
    
    fn handle_stream(stream: TcpStream, tx: Sender<ServerEvent>) {
        let id = Uuid::new_v4();
        tx.send(ServerEvent::Connect{
            conn_id: id.clone(),
            conn: stream
        });
        
        thread::spawn(move || {
            let mut buffer = [0u8; 512];
            loop {
                match stream.read(&mut buffer) {
                    Ok(size) => {
                        if size == 0 {
                            break;
                        }
                    },
                    Err(e) => {/*todo*/}
                }
            }
            tx.send(ServerEvent::Close{
                conn_id: id
            });
        });
    }

    fn listen(&mut self) -> Receiver<ServerEvent> {
        let listener = TcpListener::bind(self.addr).unwrap();
        let (tx, rx) = channel();
        self.tx = Some(tx);
        thread::spawn(move || {
            for t in listener.incoming() {
                if let Ok(stream) = t {
                    Server::handle_stream(stream, tx.clone());
                }
            }
        });
        rx
   }
}