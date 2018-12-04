use std::sync::mpsc::Receiver;

mod event;
mod memory_evhub;

pub trait Evhub : Sync + Send {
    fn subscribe(&mut self) -> Receiver<EventSend>;
    fn publish(&mut self, to: &str, ev: Event);
}

pub use self::event::{Event, EventSend};
pub use self::memory_evhub::MemoryEvhub;