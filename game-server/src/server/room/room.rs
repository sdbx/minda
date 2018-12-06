use uuid::Uuid;

pub struct Room {
    pub name: String
}

pub struct User {
    pub conn_id: Uuid
}