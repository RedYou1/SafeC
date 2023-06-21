use crate::{class::Class, compilable::Compilable};

pub struct Type {
    pub of: &'static mut Class,
    pub own: bool,
    pub is_ref: bool,
    pub nullable: bool,
    pub can_be_children: bool,
    pub can_call_func: bool,
}

impl Type {
    pub unsafe fn compile(&mut self) -> String {
        if self.of.compile().is_err() {
            panic!("class compile error");
        }

        return format!(
            "{}{}{}",
            if self.is_ref { "*" } else { "" },
            self.of.name,
            if self.nullable { "?" } else { "" }
        );
    }
}
