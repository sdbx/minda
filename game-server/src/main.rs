extern crate redis;
extern crate uuid;
extern crate serde;
extern crate ticker;
extern crate serde_json;
#[macro_use]
extern crate log;
#[macro_use]
extern crate serde_derive;
#[macro_use]
extern crate quick_error;
#[macro_use]
mod tool;
extern crate chrono;
extern crate simplelog;
use simplelog::*;

mod error;
mod model;
mod game;
mod server;

use server::{Server};

fn main() {
    CombinedLogger::init(vec![TermLogger::new(LevelFilter::Info, Config::default()).unwrap()]).unwrap();
    let server = Server::new("0.0.0.0:5353", "test", "127.0.0.1:5353", "redis://127.0.0.1/");
    server.serve();
}

