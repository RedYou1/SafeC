mod _type;
mod class;
mod my_file_reader;
mod object;

use regex::Regex;
use std::collections::HashMap;
use std::fs::File;
use std::io::{Error, Write};

use class::Class;
use my_file_reader::FileReader;

fn class_declaration(
    lines: &mut FileReader,
    classes: &mut HashMap<&str, Class>,
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
    println!("class {}", c.get(1).unwrap().as_str());
}

fn main() -> Result<(), Error> {
    let mut classes: HashMap<&str, Class> = HashMap::from([
        (
            "i32",
            Class {
                name: "int",
                variables: HashMap::new(),
            },
        ),
        (
            "f32",
            Class {
                name: "float",
                variables: HashMap::new(),
            },
        ),
    ]);

    let tokens = HashMap::from([(r"^([a-zA-Z]{1}[a-zA-Z0-9]*):$", &class_declaration)]);

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
