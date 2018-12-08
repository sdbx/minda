use game::Game;
use model::Event;
use server::Server;
use std::collections::HashMap;
use uuid::Uuid;
use model::User;

pub struct Room {
    pub name: String,
    pub users: HashMap<String, RoomUser>,
    pub game: Option<Game>
}

impl Room {
    pub fn new(name: String) -> Self {
        Self {
            name: name,
            users: HashMap::new(),
            game: None
        }
    }

    pub fn add_user(&mut self, id: &str, conn_id: Uuid, user: User) {
        self.users.insert(id.to_owned(), RoomUser {
            conn_id: conn_id,
            user: user
        });
    }

    pub fn get_username_or_empty(&self, id: &str) -> String {
        match self.users.get(id) {
            Some(user) => user.user.username.clone(),
            None => "".to_owned()
        }
    }
}

pub struct RoomUser {
    pub conn_id: Uuid,
    pub user: User
}