use std::collections::HashMap;
use std::sync::mpsc::{Sender, Receiver};
use std::sync::{Arc, Mutex};
use std::sync::mpsc;
use evhub::{EventSend, ServerEvent, ServerEventSend};
use std::thread;
use game::{Game, Board};

pub type SendMap = HashMap<&'static str, Sender<EventSend>>;

pub struct Server {
    game: Game,
    sends: SendMap
}

impl Server {
    pub fn start(recv: Receiver<ServerEventSend>, sends: SendMap) {
        let serv = Arc::new(Mutex::new(Server::new(sends)));

        let s = serv.clone();
        thread::spawn(move || {
            loop {
                if let Ok(es) = recv.recv() {
                    s.lock().unwrap().handle_event(es)
                }
            }
        });

        loop {
        }
    }

    pub fn new_chan() -> (Sender<ServerEventSend>, Receiver<ServerEventSend>) {
        mpsc::channel()
    }

    fn new(sends: SendMap) -> Self {
        Self {
            game: Game::new(Board::test_board(), "110v".to_owned(), "babu".to_owned()),
            sends: sends,
        }
    }

    fn handle_event(&mut self, ev: ServerEventSend) {
        use self::ServerEvent::*;
        match ev.ev {
            Connect{ id: id } => {
            }
        }
    }
}