mod _type;
mod class;
mod my_file_reader;
mod object;

use regex::Regex;
use std::cell::RefCell;
use std::collections::HashMap;
use std::fs::File;
use std::io::{Error, Write};

use class::Class;
use my_file_reader::FileReader;

static mut STRINGS: Vec<String> = vec![];
static mut ALLCLASS: Vec<Class<'static>> = vec![];

fn add_class<'a>(c: Class<'a>) -> &'static Class<'static> {
    unsafe {
        let i = ALLCLASS.len();
        let i2 = STRINGS.len();
        STRINGS.push(String::from(c.name));
        ALLCLASS.push(Class { name: &STRINGS[i2], variables: c.variables, extends: c.extends, implements: c.implements });
        return &ALLCLASS[i];
    }
}

fn class_declaration(
    lines: &mut FileReader,
    classes: &mut RefCell<HashMap<&'static str, &'static Class<'static>>>,
    c: regex::Captures,
) {
    lines.next();
    while lines.current.is_some() {
        let line = lines.current.as_deref().unwrap();
        if line.len() == 0 {
            lines.next();
            continue;
        }
        if !line.starts_with("\t") {
            break;
        }
        lines.next();
    }

    let name = c.get(1).unwrap().as_str();
    let mut extends: Option<&Class> = None;
    let mut implements: Vec<&Class> = vec![];

    print!("class {}", name);
    if c.get(3).is_some() {
        let m: Vec<&str> = c.get(3).unwrap().as_str().split(", ").collect();

        match classes.get_mut().get(m[0]) {
            Some(l) => extends = Some(l),
            None => panic!("extends of class {} not found", name),
        }

        if m.len() > 1 {
            implements = Vec::with_capacity(m.len() - 1);
            for i in 1..m.len() {
                let t = classes.get_mut().get(m[i]);
                if extends.is_none() {
                    panic!("implements of class {} not found", name);
                }
                implements.push(t.unwrap());
            }
        }
    }

    let class = add_class(Class {
        name: name,
        variables: HashMap::new(),
        extends: extends,
        implements: implements,
    });

    classes.get_mut().insert(class.name, class);

    println!();
}

fn main() -> Result<(), Error> {
    let mut classes = RefCell::from(HashMap::from([
        (
            "i32",
            add_class(Class {
                name: "int",
                variables: HashMap::new(),
                extends: None,
                implements: vec![],
            }),
        ),
        (
            "f32",
            add_class(Class {
                name: "float",
                variables: HashMap::new(),
                extends: None,
                implements: vec![],
            }),
        ),
    ]));

    let tokens = HashMap::from([(
        r"^([a-zA-Z]{1}[a-zA-Z0-9]*)(\((([a-zA-Z]{1}[a-zA-Z0-9]*)(, ([a-zA-Z]{1}[a-zA-Z0-9]*))*)\)){0,1}:$",
        &class_declaration,
    )]);

    let mut lines = FileReader::new("..\\testRedRust\\main.rr").unwrap();

    while lines.current.as_deref().is_some() {
        let line = String::from(lines.current.as_deref().unwrap());
        let mut captured = tokens
            .iter()
            .map(|i| (Regex::new(i.0).unwrap().captures(line.as_str()), i.1))
            .filter(|i| i.0.is_some());
        if captured.clone().count() != 1 {
            return Err(Error::new(
                std::io::ErrorKind::NotFound,
                format!("No token match {}", line),
            ));
        }
        let i = captured.next().unwrap();
        i.1(&mut lines, &mut classes, i.0.unwrap());
    }

    let mut output = File::create("..\\testC\\testC.c")?;
    write!(output, "...")?;

    Ok(())
}
