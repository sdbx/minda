use server::User;
use api::Api;
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use std::sync::mpsc;
use evhub::Evhub;
use std::thread;
use server::evhub_handler::EvhubHandler;
use server::api_event_handler::ApiEventHandler;


pub struct Server<T: Evhub + 'static> {
    users: Arc<Mutex<HashMap<String, User>>>,
    apis: Arc<Mutex<HashMap<&'static str, Box<Api>>>>,
    evhub: T
}

impl<T: Evhub + 'static> Server<T> {
    pub fn new(evhub: T) -> Self {
        Self {
            users: Arc::new(Mutex::new(HashMap::new())),
            apis: Arc::new(Mutex::new(HashMap::new())),
            evhub: evhub,
        }
    }

    pub fn add_api(&mut self, api: Box<Api>) {
        self.apis.lock().unwrap().insert(api.kind(), api);
    }

    pub fn run(&self) {
        let (event_tx, event_rv) = mpsc::channel();

        {
            let mut apis = self.apis.lock().unwrap();
            for (_, api) in apis.iter() {
                api.run(event_tx.clone());
            }
        }

        let evhub_handler = EvhubHandler::new(self.evhub, self.users.clone(), self.apis.clone());
        thread::spawn(move || {
            evhub_handler.run();
        });

        let api_event_handler = ApiEventHandler::new(event_rv);
        thread::spawn(move || {
            api_event_handler.run();
        });
    }
}