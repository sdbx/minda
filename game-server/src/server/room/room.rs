use error::Error;
use std::collections::HashSet;
use chrono::Utc;
use chrono::DateTime;
use game::Game;
use model::Event;
use server::Server;
use std::collections::HashMap;
use uuid::Uuid;
use game::{Board};
use model::{User, UserId, RoomConf, RoomRank, Room as MRoom};

pub struct Room {
    pub id: String,
    pub created_at: DateTime<Utc>,
    pub conf: RoomConf,
    pub rank: Option<RoomRank>,
    pub users: HashMap<Uuid, RoomUser>,
    pub banned_users: HashSet<UserId>,
    pub game: Option<Game>
}

impl Room {
    pub fn new(id: &str, conf: &RoomConf, rank: Option<&RoomRank>) -> Self {
        Self {
            id: id.to_owned(),
            created_at: Utc::now(),
            conf: conf.clone(),
            rank: rank.cloned(),
            users: HashMap::new(),
            banned_users: HashSet::new(),
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

    pub fn get_users(&self, user_id: UserId) -> Vec<&RoomUser> {
        self.users.iter().filter(|(_, u)| u.user_id == user_id).map(|(_, u)| u).collect()
    }

    pub fn to_model(&self) -> MRoom {
        MRoom {
            id: self.id.clone(),
            created_at: self.created_at,
            conf: self.conf.clone(),
            users: self.users.iter().map(|(_,u)| {
                u.user_id
            }).collect::<Vec<_>>(),
            rank: self.rank.clone(),
            ingame: !self.game.is_none()
        }
    }

    pub fn start(&mut self) -> Result<(), Error> {
        let game = Game::new(self.conf.black, self.conf.white, &self.conf.map, self.conf.game_rule.clone())?;
        self.game = Some(game);
        Ok(())
    }

    pub fn set_conf(&mut self, conf: &RoomConf) -> Result<(), Error> {
        if 
            //ensure that the users in the conf exist
            (!self.exists_user(conf.black) && conf.black != UserId::empty) || 
            (!self.exists_user(conf.white) && conf.white != UserId::empty) || 
            (!self.exists_user(conf.king) && self.rank.is_none()) ||
            //prohibit a same player playing in both colors
            (conf.black == conf.white && conf.black != UserId::empty) ||
            //prohibit changing color of player while ingame
            ((self.conf.black != conf.black || self.conf.white != conf.white) && !self.game.is_none()) {
            return Err(Error::InvalidParm)
        }
        let board = Board::from_string(&conf.map)?;
        //TODO make this configurable
        if board.side() != 5 || !conf.game_rule.verify(&board) {
            return Err(Error::InvalidParm)
        }

        self.conf = conf.clone();
        Ok(())
    }
}

#[derive(Clone)]
pub struct RoomUser {
    pub conn_id: Uuid,
    pub key: String,
    pub user_id: UserId
}