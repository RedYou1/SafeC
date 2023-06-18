use crate::_type::Type;
use std::collections::HashMap;

pub struct Class {
    pub name: String,
    pub variables: HashMap<String, Type>,
    pub extends: Option<&'static Class>,
    pub implements: Vec<&'static Class>,
}

impl Class {
    pub fn compile(&self) -> String {
        return format!(
            "typedef struct {} {{\n{}\n}}{};",
            self.name,
            self.variables
                .iter()
                .map(|f| format!("{} {};", f.1.name(), f.0))
                .collect::<Vec<String>>()
                .join("\n"),
            self.name
        );
    }
}
