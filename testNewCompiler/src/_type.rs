use crate::class::Class;

pub struct Type<'a> {
    pub of: &'a Class<'a>,
    pub own: bool,
    pub is_ref: bool,
    pub nullable: bool,
    pub can_be_children: bool,
    pub can_call_func: bool,
}

impl<'a> Type<'a> {
    pub fn name(&self) -> String {
        return format!("{}", self.of.name);
    }
}
