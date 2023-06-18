use std::sync::atomic::AtomicBool;
use std::sync::atomic::Ordering::Relaxed;

use crate::captures_utils::CapturesGetStr;
use pcre2::bytes::Captures;

#[derive(Copy, Clone)]
#[allow(dead_code)]
pub enum LogLevel {
    XDEBUG = 0,
    DEBUG = 1,
    INFO = 2,
    WARNING = 3,
    ALARM = 4,
}

const CURRENTLOGLEVEL: LogLevel = LogLevel::INFO;
static LOGCURRENTLEVEL: AtomicBool = AtomicBool::new(true);

pub fn log(s: String, l: LogLevel) {
    if CURRENTLOGLEVEL as u8 <= l as u8 {
        if LOGCURRENTLEVEL.load(Relaxed) {
            match l {
                LogLevel::XDEBUG => print!("XDebug:"),
                LogLevel::DEBUG => print!("Debug:"),
                LogLevel::INFO => print!("Info:"),
                LogLevel::WARNING => print!("Warning:"),
                LogLevel::ALARM => print!("Alarm:"),
            }
        }
        LOGCURRENTLEVEL.store(false, Relaxed);
        print!("{}", s);
    }
}

pub fn logln(s: String, l: LogLevel) {
    if CURRENTLOGLEVEL as u8 <= l as u8 {
        if LOGCURRENTLEVEL.load(std::sync::atomic::Ordering::Relaxed) {
            match l {
                LogLevel::XDEBUG => print!("XDebug:"),
                LogLevel::DEBUG => print!("Debug:"),
                LogLevel::INFO => print!("Info:"),
                LogLevel::WARNING => print!("Warning:"),
                LogLevel::ALARM => print!("Alarm:"),
            }
        }
        LOGCURRENTLEVEL.store(true, Relaxed);
        println!("{}", s);
    }
}

pub fn debug_all_capture(c: &Captures) {
    for i in 1..c.len() {
        let s = c.get_str(i);
        match s {
            Some(a) => log(format!("{}&", a), LogLevel::DEBUG),
            None => log(format!("None&"), LogLevel::DEBUG),
        }
    }
    logln(String::new(), LogLevel::DEBUG);
}
