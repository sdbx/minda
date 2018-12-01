extern crate uuid;
extern crate serde;
extern crate serde_json;
#[macro_use]
extern crate serde_derive;
#[macro_use]
extern crate quick_error;
#[macro_use]
mod tool;

mod evhub;
mod game;
mod server;


use evhub::SocketHub;
use std::sync::{Arc, Mutex};
use server::{Server, SendMap};

fn main() {
    let (send, recv) = Server::new_chan();
    let mut send_map = SendMap::new();
    let send2 = SocketHub::start("0.0.0.0:8080", send.clone());
    send_map.insert(SocketHub::name, send2);
    Server::start(recv, send_map);
}

