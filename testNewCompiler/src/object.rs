use crate::_type::Type;

pub struct Object<'a>{
    pub of: &'a Type<'a>,
    pub own: bool,
    pub is_null: bool,
    pub childrens: [&'a Object<'a>]
}