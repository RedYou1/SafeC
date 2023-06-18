use crate::class::Class;

pub struct Type {
    pub of: &'static Class,
    pub own: bool,
    pub is_ref: bool,
    pub nullable: bool,
    pub can_be_children: bool,
    pub can_call_func: bool,
}

impl Type {
    pub fn name(&self) -> String {
        return format!("{}", self.of.name);
    }
}
