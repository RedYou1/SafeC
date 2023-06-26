use crate::{
    class::Class,
    compilable::Compilable,
    writable::{Writable, WriteType},
};

pub struct Type {
    pub of: &'static mut Class,
    pub own: bool,
    pub is_ref: bool,
    pub nullable: bool,
    pub can_be_children: bool,
    pub can_call_func: bool,
}

impl Writable for Type {
    unsafe fn write(&'static mut self) -> String {
        let name = String::from(self.of.name.as_str());
        if self.of.compile().is_err() {
            panic!("class compile error");
        }

        return format!(
            "{}{}{}",
            if self.is_ref { "*" } else { "" },
            name,
            if self.nullable { "?" } else { "" }
        );
    }

    fn get_type(&'static mut self) -> WriteType {
        return WriteType::Type(self);
    }
}
