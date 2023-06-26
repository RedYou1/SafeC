use crate::{
    compilable::{Compilable, OUTPUT},
    object::Object,
    writable::{Writable, WriteType},
};
use std::{
    collections::HashMap,
    io::{Error, Write},
};

pub struct Class {
    pub included: bool,
    pub to_include: Vec<&'static mut dyn Compilable>,

    pub name: String,
    pub variables: HashMap<String, Object<'static>>,
    pub extends: Option<&'static mut Class>,
    pub implements: Vec<&'static mut Class>,
}

struct Var {
    name: &'static String,
    obj: &'static mut Object<'static>,
}

impl Class {
    fn variables(&'static mut self) -> Vec<Var> {
        let mut r: Vec<Var>;

        match self.extends.as_mut() {
            Some(e) => r = e.variables(),
            None => r = Vec::new(),
        }

        let mut temp = self
            .variables
            .iter_mut()
            .map(|i| Var {
                name: i.0,
                obj: i.1,
            })
            .collect::<Vec<Var>>();
        r.append(&mut temp);

        return r;
    }
}

impl Writable for Class {
    unsafe fn write(&mut self) -> String {
        return format!("{}", self.name);
    }

    fn get_type(&'static mut self) -> WriteType {
        WriteType::Class(self)
    }
}

impl Compilable for Class {
    unsafe fn compile(&'static mut self) -> Result<(), Error> {
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

        let name = String::from(self.name.as_str());

        let mut vars = self.variables();

        let varsNames = vars
            .iter_mut()
            .map(|f| format!("{} {};", f.obj.of.write(), f.name))
            .collect::<Vec<String>>()
            .join("\n");

        return OUTPUT.unwrap().as_mut().unwrap().write_fmt(format_args!(
            "typedef struct {} {{\n{}\n}}{};\n",
            name, varsNames, name
        ));
    }
}
