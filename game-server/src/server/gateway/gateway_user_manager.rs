use model::User;
use std::collections::HashMap;
use std::collections::HashSet;
use std::sync::{Arc, Mutex};

pub struct GatewayUserManager {
    users: HashMap<String, User>,
    invited_users: HashSet<String>
}

impl GatewayUserManager {
    pub fn new() -> Arc<Mutex<Self>> {
        Arc::new(Mutex::new(Self {
            users: HashMap::new(),
            invited_users: HashSet::new()
        }))
    }

    pub fn get(&self, id: &str) -> Option<&User> {
        self.users.get(id)
    }
}