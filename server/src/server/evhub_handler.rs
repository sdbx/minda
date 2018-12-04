use server::User;
use api::Api;
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use evhub::Evhub;

pub struct EvhubHandler<T: Evhub> {
    evhub: T,
    users: Arc<Mutex<HashMap<String, User>>>,
    apis: Arc<Mutex<HashMap<&'static str, Box<Api>>>>,
}

impl<T: Evhub> EvhubHandler<T> {
    pub fn new(evhub: T, 
        users: Arc<Mutex<HashMap<String, User>>>, 
        apis: Arc<Mutex<HashMap<&'static str, Box<Api>>>>) -> Self {
        Self {
            evhub,
            users,
            apis,
        }
    }

    pub fn run(&self) {
        let recv = self.evhub.subscribe();
        while let Ok(ev) = recv.recv() {
            if let Some(user) = self.users.lock().unwrap().get(&ev.user_id) {
                let apis = self.apis.lock().unwrap();
                let api = apis.get(user.api_kind).unwrap();
                api.dispatch(&ev.user_id, ev.ev);
            }
        }
    }
}