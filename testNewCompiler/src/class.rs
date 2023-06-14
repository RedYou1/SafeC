use crate::_type::Type;
use std::collections::HashMap;

pub struct Class<'a> {
    pub name: &'a str,
    pub variables: HashMap<&'a str, Type<'a>>,
}

impl<'a> Class<'a> {
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
