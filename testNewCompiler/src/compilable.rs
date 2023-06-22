use std::fs::File;
use std::io::Error;

use crate::class::Class;
use crate::func::Func;

pub trait Compilable {
    unsafe fn compile(&mut self) -> Result<(), Error>;
    fn get_type(&'static mut self) -> CompilType;
}

pub static mut OUTPUT: Option<*mut File> = None;

pub enum CompilType {
    Class(&'static mut Class),
    Func(&'static mut Func),
}
