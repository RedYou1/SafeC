use crate::class::Class;

pub struct Func<'a> {
    pub name: &'a str,
    pub return_type: Option<&'static Class<'static>>,
    pub params: Vec<(&'static Class<'static>, &'static str)>,
}
