use model::User;
use model::Event;
use game::{Game, Player, Board, Cord, Stone};
use std::sync::mpsc::Sender;
use std::sync::mpsc::channel;
use std::sync::mpsc::Receiver;
use uuid::Uuid;
use std::net::{TcpStream, TcpListener};
use model::{parse_command, Command, Invite};
use server::room::Room;
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use std::io::{Read, Write};
use std::thread;
use server::cmd;

pub enum ServerEvent {
    Connect{ conn_id: Uuid, conn: TcpStream },
    Close { conn_id: Uuid },
    Command { conn_id: Uuid, cmd: Command }
}

#[derive(Clone, Debug)]
pub struct Connection {
    pub conn_id: Uuid,
    pub id: Option<String>,
    pub room: Option<String>
}

pub struct Server {
    tx: Option<Sender<ServerEvent>>,
    addr: String,
    pub rooms: HashMap<String, Room>,
    pub invites: HashMap<String, Invite>,
    pub conns: HashMap<Uuid, Connection>,
    pub streams: HashMap<Uuid, TcpStream>
}

fn test_board() -> Board {
    let mut board = Board::new(5);
    board.set(Cord(0,0,0), Stone::Black);
    board.set(Cord(0,-3,3), Stone::White);
    board.set(Cord(0,-3,3), Stone::Black);
    board.set(Cord(-1,-2,3), Stone::Black);
    board.set(Cord(-2,-1,3), Stone::White);
    board.set(Cord(-3,0,3), Stone::White);
    board
}

fn test_game() -> Game {
    Game {
        black: "black".to_owned(),
        white: "white".to_owned(),
        board: test_board(),
        turn: Player::Black
    }
}

fn test_rooms() -> HashMap<String, Room> {
    let mut t = HashMap::new();
    let mut room = Room::new("test".to_owned());
    room.game = Some(test_game());
    t.insert("test".to_owned(), room);
    t
}

fn test_invites() -> HashMap<String, Invite> {
    let mut t = HashMap::new();
    t.insert("black".to_owned(), Invite {
        id: "black".to_owned(),
        user: User{
            username: "110vBABU".to_owned()
        },
        room: "test".to_owned()
    });
    t.insert("white".to_owned(), Invite {
        id: "white".to_owned(),
        user: User{
            username: "asd".to_owned()
        },
        room: "test".to_owned()
    });
    t
}

impl Server {
    pub fn new(addr: &str) -> Self {
        Self {
            tx: None,
            addr: addr.to_owned(),
            rooms: test_rooms(),
            invites: test_invites(),
            conns: HashMap::new(),
            streams: HashMap::new()
        }
    }

    pub fn serve(mut self) {
        for event in self.listen().iter() {
            match event {
                ServerEvent::Connect{ conn_id, conn } => {
                    info!("client({}) connected", conn_id);
                    self.conns.insert(conn_id, Connection {
                        conn_id: conn_id,
                        id: None,
                        room: None
                    });
                    self.streams.insert(conn_id, conn);
                },
                ServerEvent::Command { conn_id, cmd } => {
                    info!("client({}) command", conn_id);
                    let conn = { 
                        self.conns.get(&conn_id).unwrap().clone()
                    };
                    if let Err(err) = cmd::handle(&mut self, &conn, cmd) {
                        self.dispatch(conn_id, Event::Error{message: format!("{}", err)});
                    };
                },
                _ => { }
            }
        }
    }
    
    pub fn dispatch(&mut self, conn_id: Uuid, event: Event) {
        if let Some(stream) = self.streams.get_mut(&conn_id) {
            let msg = serde_json::to_string(&event).unwrap() + "\n";
            info!("client({}) will receive msg: {}", conn_id, msg);
            stream.write(msg.as_bytes());
            stream.flush();
        }
    }

    fn handle_stream_loop(conn_id: Uuid, tx: &Sender<ServerEvent>, mut stream: TcpStream) {
        let mut buffer = [0u8; 512];
        loop {
            match stream.read(&mut buffer) {
                Ok(size) => {
                    if size == 0 {
                        break;
                    }
                    let msg = String::from_utf8_lossy(&buffer);
                    info!("client({}) sent msg: {}", conn_id, msg);
                    let t = parse_command(&msg.trim_matches('\0'));
                    if let Ok(cmd) = t {
                        info!("{:?}", cmd);
                        tx.send(ServerEvent::Command{
                            conn_id,
                            cmd
                        });
                    }
                },
                Err(e) => {
                    error!("{}", e);
                }
            }
        }
    }
    
    fn handle_stream(mut stream: TcpStream, tx: Sender<ServerEvent>) {
        let conn_id = Uuid::new_v4();
        tx.send(ServerEvent::Connect{
            conn_id: conn_id,
            conn: stream.try_clone().unwrap()
        });
        
        thread::spawn(move || {
            Server::handle_stream_loop(conn_id, &tx, stream);
            tx.send(ServerEvent::Close{
                conn_id
            });
        });
    }

    fn listen(&mut self) -> Receiver<ServerEvent> {
        let listener = TcpListener::bind(&self.addr).unwrap();
        let (tx, rx) = channel();
        self.tx = Some(tx.clone());
        thread::spawn(move || {
            for t in listener.incoming() {
                if let Ok(mut stream) = t {
                    Server::handle_stream(stream, tx.clone());
                }
            }
        });
        rx
   }

    pub fn broadcast(&mut self, room: &str, event: Event) {
        let conn_ids = {
            let room = self.rooms.get(room).unwrap();
            room.users.iter().map(|t| t.1.conn_id.clone()).collect::<Vec<_>>()
        };
        conn_ids.iter().for_each(|conn_id| self.dispatch(*conn_id, event.clone()));
    }
}