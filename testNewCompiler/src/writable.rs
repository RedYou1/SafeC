use crate::{_type::Type, class::Class, func::Func, object::Object};

pub trait Writable {
    unsafe fn write(&'static mut self) -> String;
    fn get_type(&'static mut self) -> WriteType;
}

pub enum WriteType {
    Class(&'static mut Class),
    Func(&'static mut Func),
    Type(&'static mut Type),
    Object(&'static mut Object<'static>),
}
