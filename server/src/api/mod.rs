mod api_event;
use std::sync::mpsc::Sender;
use evhub::Event;

pub trait Api: Sync + Send {
    fn run(&self, event_tx: Sender<ApiEventSend>);
    fn dispatch(&self, to: &str, ev: Event);
    fn kind(&self) -> &'static str;
}

pub mod socket;
pub use self::api_event::{ApiEventSend, ApiEvent};