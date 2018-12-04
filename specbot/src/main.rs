use std::{env};

#[macro_use]
extern crate serenity;

mod bot;

fn main() {
    let token = env::var("DISCORD_TOKEN").expect(
        "Expected a token in env var"
    );
    
    if let Err(why) = bot::client(&token).start() {
        println!("Client error: {:?}", why);
    }
}
