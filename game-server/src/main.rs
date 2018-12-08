extern crate uuid;
extern crate serde;
extern crate serde_json;
#[macro_use]
extern crate serde_derive;
#[macro_use]
extern crate quick_error;
#[macro_use]
mod tool;

mod model;
mod board;
mod server;


use server::{Server};

fn main() {
    let server = Server::new("0.0.0.0:5353");
    server.serve();
}

