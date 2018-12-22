quick_error! {
    #[derive(Debug)]
    pub enum Error {
        NoneError(err: std::option::NoneError) {
            from()
            description("none error")
            display("none error")
        }
        JsonError(err: serde_json::Error) {
            from()
            description("json error")
            display("json error: {}", err)
            cause(err)
        }
        RedisError(err: redis::RedisError) {
            from()
            description("redis error")
            display("redis error: {}", err)
            cause(err)
        }
        Internal {
            description("Internal error")
        }
        InvalidCommand {
            description("Invalid command")
        }
        InvalidCord {
            description("Invalid coordinates")
        }
        InvalidVec {
            description("Invalid vec")
        }
        InvalidMove {
            description("Invalid move")
        }
    }
}