use crate::_type::Type;

pub struct Func {
    pub name: String,
    pub return_type: Option<Type>,
    pub params: Vec<(Type, String)>,
}
