use crate::_type::Type;

pub struct Object{
    pub of: &'static Type,
    pub own: bool,
    pub is_null: bool,
    pub childrens: [&'static Object]
}