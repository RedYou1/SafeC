use std::fs::File;
use std::io::Error;

pub trait Compilable {
    unsafe fn compile(&'static mut self) -> Result<(), Error>;
}

pub static mut OUTPUT: Option<*mut File> = None;
