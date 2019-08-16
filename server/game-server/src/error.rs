quick_error! {
    #[derive(Debug)]
    pub enum Error {
        NoneError(err: std::option::NoneError) {
            from()
            description("none error")
            display("none error")
        }
        Permission{
            description("permission lacked")
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
        TaskError(error: String) {
            description("task error")
            display("task error: {}", error)
        }
        ShouldTerminate(err: String) {
            description("should terminate")
            display("{}", err)
        }
        GameStarted {
            description("Game have benn started already")
        }
        RoomNotEmpty {
            description("Room not empty")
        }
        Internal {
            description("Internal error")
        }
        Banned {
            description("You're banned")
        }
        InvalidParm {
            description("Invalid parameters")
        }
        InvalidState {
            description("Invalid state")
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