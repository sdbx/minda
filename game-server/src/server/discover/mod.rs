use redis::Client;
use redis::Commands;
use server::Server;
use error::Error;
use model::GameServer;

const redis_server_hash: &'static str = "game_server_hash";

pub fn update(server: &mut Server) -> Result<(), Error> {
    let conn = server.redis.get_connection()?;
    let game_server = GameServer::from_server(server);
    let buf = serde_json::to_string(&game_server)?;
    let _: () = conn.hset(redis_server_hash, &server.name, &buf)?;
    Ok(())
}