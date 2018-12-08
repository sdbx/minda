quick_error! {
    #[derive(Debug)]
    pub enum Error {
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