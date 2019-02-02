use std::ops::Deref;
use serde::{Serialize, Serializer, Deserialize, Deserializer};
use game::Cord;
use chrono::Utc;
use chrono::DateTime;

#[derive(Debug, Copy, Clone, PartialEq)]
pub struct UserId(isize);

impl UserId {
    pub const empty: UserId = UserId(-1);

    pub fn to_isize(&self) -> isize {
        self.0
    }

    pub fn to_user(&self) {
        //TODO
    }
}

impl Serialize for UserId {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
        where S: Serializer
    {
        self.0.serialize(serializer)
    }
}

impl<'de> Deserialize<'de> for UserId {
    fn deserialize<D>(deserializer: D) -> Result<Self, D::Error>
        where D: Deserializer<'de>
    {
        Deserialize::deserialize(deserializer)
            .map(|id: isize| UserId(id))
    }
}

#[derive(Deserialize, Serialize, Copy, Clone, PartialEq, Debug)]
pub struct AxialCord {
    pub x: isize,
    pub y: isize,
    pub z: isize
}

impl AxialCord {
    pub fn to_cord(&self) -> Cord {
        Cord(self.x, self.y, self.z)
    }
}

#[derive(Clone, Serialize, Deserialize, Debug)]
pub struct User {
    pub username: String
}

#[derive(Serialize, PartialEq, Deserialize, Debug, Clone)]
pub struct RoomConf {
    pub name: String,
    pub king: UserId,
    pub black: UserId,
    pub white: UserId 
}

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct Room {
    pub id: String,
    pub created_at: DateTime<Utc>,
    pub conf: RoomConf,
    pub users: Vec<UserId>,
    pub ingame: bool
}