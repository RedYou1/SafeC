use std::io::Error;
use std::io::Write;

use crate::_type::Type;
use crate::compilable::CompilType;
use crate::compilable::Compilable;
use crate::compilable::OUTPUT;

pub struct Func {
    pub included: bool,
    pub to_include: Vec<&'static mut dyn Compilable>,

    pub name: String,
    pub return_type: Option<Type>,
    pub params: Vec<(Type, String)>,
}

impl Compilable for Func {
    fn get_type(&'static mut self) -> CompilType {
        CompilType::Func(self)
    }

    unsafe fn compile(&mut self) -> Result<(), Error> {
        if self.included {
            return Ok(());
        }
        self.included = true;

        for i in self.to_include.iter_mut() {
            i.compile()?;
        }

        return OUTPUT.unwrap().as_mut().unwrap().write_fmt(format_args!(
            "{} {}({}){{\n\n}}\n",
            match self.return_type.as_mut() {
                Some(l) => l.compile(),
                None => "void".to_string(),
            },
            self.name,
            self.params
                .iter_mut()
                .map(|f| format!("{} {};", f.0.compile(), f.1))
                .collect::<Vec<String>>()
                .join("\n")
        ));
    }
}
