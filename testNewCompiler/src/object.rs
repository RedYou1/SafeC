use crate::{_type::Type, writable::Writable};

pub struct Object<'a> {
    pub of: Type,
    pub own: bool,
    pub is_null: bool,
    pub childrens: Vec<&'a Object<'a>>,
}

impl<'a> Writable for Object<'a>{
    unsafe fn write(&mut self) -> String {
        todo!()
    }

    fn get_type(&'static mut self) -> crate::writable::WriteType {
        todo!()
    }
}