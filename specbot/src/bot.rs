use serenity::{
    framework::standard::{StandardFramework},
    prelude::*,
    model::{gateway::Ready}
};

struct Handler;

impl EventHandler for Handler {
    fn ready(&self, _: Context, ready: Ready) {
        println!("{} is connected!", ready.user.name);
    }

    fn 
}

pub fn client(token: &str) -> Client {
    let mut cli = Client::new(token, Handler).expect("Err creating client");
    cli.with_framework(framework());
    cli
}

fn framework() -> StandardFramework {
    StandardFramework::new()
    .configure(|c| c
        .allow_whitespace(true)
        .on_mention(true)
        .prefix(">")
    )
    .command("ping", |c| c
        .cmd(ping)
    )
}

command!(ping(_ctx, msg, _args) {
    if let Err(why) = msg.channel_id.say("Pong!") {
        println!("Error sending message: {:?}", why);
    }
});