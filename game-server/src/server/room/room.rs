use chrono::Utc;
use chrono::DateTime;
use game::Game;
use model::Event;
use server::Server;
use std::collections::HashMap;
use uuid::Uuid;
use model::{User, UserId, RoomConf, Room as MRoom};

pub struct Room {
    pub id: String,
    pub created_at: DateTime<Utc>,
    pub conf: RoomConf,
    pub users: HashMap<Uuid, RoomUser>,
    pub game: Option<Game>
}

impl Room {
    pub fn new(conf: &RoomConf) -> Self {
        Self {
            id: Uuid::new_v4().to_string(),
            created_at: Utc::now(),
            conf: conf.clone(),
            users: HashMap::new(),
            game: None
        }
    }

    pub fn add_user(&mut self, conn_id: Uuid, user_id: UserId, key: &str) {
        self.users.insert(conn_id.clone(), RoomUser {
            conn_id: conn_id,
            key: key.to_owned(),
            user_id: user_id
        });
    }

    pub fn exists_user(&self, user_id: UserId) -> bool {
        match self.get_user(user_id) {
            Some(_) => true,
            None => false
        }
    }

    pub fn get_user(&self, user_id: UserId) -> Option<&RoomUser> {
        for (_, u) in self.users.iter() {
            if u.user_id == user_id {
                return Some(u)
            }
        }
        None
    }

    pub fn to_model(&self) -> MRoom {
        MRoom {
            id: self.id.clone(),
            created_at: self.created_at,
            conf: self.conf.clone(),
            users: self.users.iter().map(|(_,u)| {
                u.user_id
            }).collect::<Vec<_>>()
        }
    }
}

pub struct RoomUser {
    pub conn_id: Uuid,
    pub key: String,
    pub user_id: UserId
}