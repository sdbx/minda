extern crate uuid;
extern crate serde;
extern crate serde_json;
#[macro_use]
extern crate log;
#[macro_use]
extern crate serde_derive;
#[macro_use]
extern crate quick_error;
#[macro_use]
mod tool;
extern crate simplelog;
use simplelog::*;

mod model;
mod board;
mod server;


use server::{Server};


fn main() {
    CombinedLogger::init(vec![TermLogger::new(LevelFilter::Info, Config::default()).unwrap()]).unwrap();
    info!("asdfasdf");
    let server = Server::new("0.0.0.0:5353");
    server.serve();
}

