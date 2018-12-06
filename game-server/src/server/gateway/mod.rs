mod gateway_event;
mod gateway_event_handler;
mod gateway_user_manager;
mod gateway_manager;

use std::sync::mpsc::Sender;
use std::sync::{Arc, Mutex};
use model::{Event};

pub trait Gateway: Sync + Send {
    fn run(&self, event_tx: Sender<GatewayEventSend>, user_manager: Arc<Mutex<GatewayUserManager>>);
    fn dispatch(&self, to: &str, ev: Event);
    fn kind(&self) -> &'static str;
}

pub use self::gateway_event::*;
pub use self::gateway_event_handler::GatewayEventHandler;
pub use self::gateway_manager::GatewayManager;
pub use self::gateway_user_manager::GatewayUserManager;

pub mod socket;