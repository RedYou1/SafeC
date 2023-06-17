use crate::captures_utils::CapturesGetStr;
use pcre2::bytes::Captures;

#[derive(Copy, Clone)]
#[allow(dead_code)]
pub enum LogLevel {
    DEBUG = 0,
    INFO = 1,
    WARNING = 2,
    ALARM = 3,
}

const CURRENTLOGLEVEL: LogLevel = LogLevel::INFO;

pub fn log(s: String, l: LogLevel) {
    if CURRENTLOGLEVEL as u8 <= l as u8 {
        print!("{}", s);
    }
}

pub fn logln(s: String, l: LogLevel) {
    if CURRENTLOGLEVEL as u8 <= l as u8 {
        println!("{}", s);
    }
}

pub fn debug_all_capture(c: Captures) {
    for i in 1..c.len() {
        let s = c.get_str(i);
        match s {
            Some(a) => log(format!("{}&", a), LogLevel::DEBUG),
            None => log(format!("None&"), LogLevel::DEBUG),
        }
    }
    logln(String::new(), LogLevel::DEBUG);
}
