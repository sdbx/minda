use std::sync::Mutex;
use std::sync::Arc;
use std::sync::mpsc::{Receiver, Sender};
use std::sync::mpsc;
use evhub::{Evhub, Event, EventSend};

pub struct MemoryEvhub {
    tx: Arc<Mutex<Vec<Sender<EventSend>>>>,
}

impl MemoryEvhub {
    pub fn new() -> Self {
        Self {
            tx: Arc::new(Mutex::new(Vec::new())),
        }
    }
}

impl Evhub for MemoryEvhub {
    fn subscribe(&mut self) -> Receiver<EventSend> {
        let (tx, rx) = mpsc::channel();
        self.tx.lock().unwrap().push(tx);
        rx
    }

    fn publish(&mut self, to: &str, ev: Event) {
        self.tx.lock().unwrap().iter().for_each(|t| {
            t.send(EventSend{user_id: to.to_owned(), ev: ev});
        });
    }
}