use std::collections::HashMap;
use std::collections::HashSet;
use std::sync::{Arc, Mutex};

pub struct GatewayUser {
    pub id: String,
    pub kind: &'static str
}

pub struct GatewayUserManager {
    users: HashMap<String, GatewayUser>,
    invited_users: HashSet<String>
}

impl GatewayUserManager {
    pub fn new() -> Arc<Mutex<Self>> {
        Arc::new(Mutex::new(Self {
            users: HashMap::new(),
            invited_users: HashSet::new()
        }))
    }

    pub fn get(&self, id: &str) -> Option<&GatewayUser> {
        self.users.get(id)
    }
}