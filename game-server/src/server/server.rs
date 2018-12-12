use model::User;
use server::task;
use model::Event;
use std::time::Duration;
use model::{Task, TaskResult, TaskRequest};
use redis::Commands;
use error::Error;
use ticker::Ticker;
use game::{Game, Player, Board, Cord, Stone};
use std::sync::mpsc::Sender;
use std::sync::mpsc::channel;
use server::discover;
use std::sync::mpsc::Receiver;
use redis::{Client, PubSub};
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
    Updated,
    Command { conn_id: Uuid, cmd: Command },
    TaskRequest { task_request: TaskRequest }
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
    pub real_addr: String,
    pub name: String,
    pub redis: Client,
    pub rooms: HashMap<String, Room>,
    pub invites: HashMap<String, Invite>,
    pub conns: HashMap<Uuid, Connection>,
    pub streams: HashMap<Uuid, TcpStream>
}

const redis_result_pubsub: &'static str = "task_result_pub_sub";
const redis_lobby_queue: &'static str = "task_lobby_queue";

fn redis_game_queue(server: &str) -> String {
    format!("task_game_queue_{}", server)
} 

fn redis_result_channel(id: &str) -> String {
    format!("task_result_chan_{}", id)
} 

impl Server {
    pub fn new(addr: &str, name: &str, real_addr: &str, redis_addr: &str) -> Self {
        Self {
            tx: None,
            name: name.to_owned(),
            addr: addr.to_owned(),
            real_addr: real_addr.to_owned(),
            redis: Client::open(redis_addr).unwrap(),
            rooms: HashMap::new(),
            invites: HashMap::new(),
            conns: HashMap::new(),
            streams: HashMap::new()
        }
    }

    pub fn tx(&self) -> &Sender<ServerEvent> {
        self.tx.as_ref().unwrap()
    }

    pub fn make_tx(&self) -> Sender<ServerEvent> {
        let tx = self.tx.as_ref().unwrap();
        tx.clone()
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
                        self.dispatch(conn_id, &Event::Error{message: format!("{}", err)});
                    };
                },
                ServerEvent::Updated => {
                    discover::update(&mut self);
                },
                ServerEvent::TaskRequest { task_request } => {
                    match task::handle(&mut self, task_request.task) {
                        Ok(res) => {
                            info!("task({}) was successful", task_request.id);
                            self.send_result(&task_request.id, &TaskResult{
                                error: None,
                                value: res
                            });
                        },
                        Err(e) => {
                            info!("task({}) encounterd an error: {}", task_request.id, e);
                            self.send_result(&task_request.id, &TaskResult{
                                error: Some(format!("{}", e)),
                                value: "".to_owned()
                            });
                        }
                    };
                },
                _ => { }
            }
        }
    }
    
    pub fn dispatch(&mut self, conn_id: Uuid, event: &Event) {
        if let Some(stream) = self.streams.get_mut(&conn_id) {
            let msg = serde_json::to_string(&event).unwrap() + "\n";
            info!("client({}) will receive msg: {}", conn_id, msg);
            stream.write(msg.as_bytes());
            stream.flush();
        }
    }

    pub fn broadcast(&mut self, room: &str, event: &Event) {
        let conn_ids = {
            let room = self.rooms.get(room).unwrap();
            room.users.iter().map(|t| t.1.conn_id.clone()).collect::<Vec<_>>()
        };
        conn_ids.iter().for_each(|conn_id| self.dispatch(*conn_id, &event));
    }

    fn ping_update(&self) {
        let tx = self.make_tx();
        thread::spawn(move || {
            tx.send(ServerEvent::Updated);
            for _ in Ticker::new(0.., Duration::from_secs(5)) {
                tx.send(ServerEvent::Updated);
            }
        });
    }

    fn handle_stream_loop(conn_id: Uuid, tx: &Sender<ServerEvent>, mut stream: TcpStream) {
        let mut buffer = [0u8; 512];
        loop {
            match stream.read(&mut buffer) {
                Ok(size) => {
                    if size == 0 {
                        break;
                    }
                    let msg = String::from_utf8_lossy(&buffer[..size]);
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
                    break;
                }
            }
        }
    }

    fn send_result(&self, id: &str, res: &TaskResult) -> Result<(), Error> {
        let conn = self.redis.get_connection()?;
        let buf = serde_json::to_string(&res)?;
        Ok(conn.rpush(redis_result_channel(id), buf)?)
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
        let (tx, rx) = channel();
        self.tx = Some(tx.clone());
        self.listen_socket();
        self.listen_task().unwrap();
        self.ping_update();
        rx
    }

    fn listen_socket(&mut self) {
        let tx = self.make_tx();
        let listener = TcpListener::bind(&self.addr).unwrap();
        thread::spawn(move || {
            for t in listener.incoming() {
                if let Ok(mut stream) = t {
                    Server::handle_stream(stream, tx.clone());
                }
            }
        });
   }

   fn listen_task(&mut self) -> Result<(), Error> {
        let mut conn = self.redis.get_connection()?;
        let name = self.name.clone();
        let tx = self.make_tx();
        thread::spawn(move || {
            loop {
                let res: Vec<String> = match conn.blpop(redis_game_queue(&name), 0) {
                    Ok(res) => res,
                    Err(e) => { error!("{}", e); continue }
                };
                let task_request: TaskRequest = match serde_json::from_str(&res.get(1).unwrap()) {
                    Ok(task) => task,
                    Err(e) => { error!("{}", e); continue }
                };
                info!("task({}) arrived: {:?}", task_request.id, task_request.task);
                tx.send(ServerEvent::TaskRequest { task_request });
            }
        });
        Ok(())
   }
}