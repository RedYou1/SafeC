use crate::{
    _type::Type,
    compilable::{CompilType, Compilable, OUTPUT},
    writable::Writable,
};
use std::{
    collections::HashMap,
    io::{Error, Write},
};

pub struct Class {
    pub included: bool,
    pub to_include: Vec<&'static mut dyn Compilable>,

    pub name: String,
    pub variables: HashMap<String, Type>,
    pub extends: Option<&'static mut Class>,
    pub implements: Vec<&'static mut Class>,
}

impl Writable for Class {
    unsafe fn write(&mut self) -> String {
        return format!("{}", self.name);
    }
}

impl Compilable for Class {
    fn get_type(&'static mut self) -> CompilType {
        CompilType::Class(self)
    }

    unsafe fn compile(&mut self) -> Result<(), Error> {
        if self.included {
            return Ok(());
        }
        self.included = true;

        if self.extends.is_some() {
            self.extends.as_mut().unwrap().compile()?;
        }

        for i in self.implements.iter_mut() {
            i.compile()?;
        }

        for i in self.to_include.iter_mut() {
            i.compile()?;
        }

        return OUTPUT.unwrap().as_mut().unwrap().write_fmt(format_args!(
            "typedef struct {} {{\n{}\n{}\n}}{};\n",
            self.name,
            match self.extends.as_mut() {
                Some(e) => e
                    .variables
                    .iter_mut()
                    .map(|f| format!("{} {};", f.1.write(), f.0))
                    .collect::<Vec<String>>()
                    .join("\n"),
                None => String::new(),
            },
            self.variables
                .iter_mut()
                .map(|f| format!("{} {};", f.1.write(), f.0))
                .collect::<Vec<String>>()
                .join("\n"),
            self.name
        ));
    }
}
