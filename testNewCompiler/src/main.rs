mod _type;
mod captures_utils;
mod class;
mod func;
mod logger;
mod my_file_reader;
mod object;

use pcre2::bytes::Captures;
use pcre2::bytes::Regex;
use std::cell::RefCell;
use std::collections::HashMap;
use std::fs::File;
use std::io::{Error, Write};

use captures_utils::CapturesGetStr;
use class::Class;
use func::Func;
use logger::debug_all_capture;
use logger::log;
use logger::logln;
#[allow(unused)]
use logger::LogLevel::ALARM;
#[allow(unused)]
use logger::LogLevel::DEBUG;
#[allow(unused)]
use logger::LogLevel::INFO;
#[allow(unused)]
use logger::LogLevel::WARNING;
use my_file_reader::FileReader;

static mut STRINGS: Vec<String> = vec![];
static mut ALLCLASS: Vec<Class<'static>> = vec![];
static mut ALLFUNC: Vec<Func<'static>> = vec![];

fn add_class<'a>(c: Class<'a>) -> &'static Class<'static> {
    unsafe {
        let i = ALLCLASS.len();
        let i2 = STRINGS.len();
        STRINGS.push(String::from(c.name));
        ALLCLASS.push(Class {
            name: &STRINGS[i2],
            variables: c.variables,
            extends: c.extends,
            implements: c.implements,
        });
        return &ALLCLASS[i];
    }
}

fn add_func<'a>(c: Func<'a>) -> &'static Func<'static> {
    unsafe {
        let i = ALLFUNC.len();
        let i2 = STRINGS.len();
        STRINGS.push(String::from(c.name));
        ALLFUNC.push(Func {
            name: &STRINGS[i2],
            return_type: c.return_type,
            params: c.params,
        });
        return &ALLFUNC[i];
    }
}

fn class_declaration(
    lines: &mut FileReader,
    classes: &mut RefCell<HashMap<&'static str, &'static Class<'static>>>,
    funcs: &mut RefCell<HashMap<&'static str, &'static Func<'static>>>,
    c: Captures,
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

    let name = c.get_str(2).unwrap();
    let mut extends: Option<&Class> = None;
    let mut implements: Vec<&Class> = vec![];

    log(format!("class {}", name), INFO);
    let m = c.get_str(7);
    if m.is_some() {
        let t = m.unwrap();
        let m: Vec<&str> = t.split(", ").collect();

        match classes.get_mut().get(m[0]) {
            Some(l) => extends = Some(l),
            None => panic!("extends of class {} not found", name),
        }

        log(format!(" extends {}", extends.unwrap().name), INFO);

        if m.len() > 1 {
            log(format!(" implements "), INFO);
            implements = Vec::with_capacity(m.len() - 1);
            for i in 1..m.len() {
                let t = classes.get_mut().get(m[i]);
                if extends.is_none() {
                    panic!("implements of class {} not found", name);
                }
                implements.push(t.unwrap());
                log(format!("{}, ", t.unwrap().name), INFO);
            }
        }
    }

    let class = add_class(Class {
        name: name.as_str(),
        variables: HashMap::new(),
        extends: extends,
        implements: implements,
    });

    classes.get_mut().insert(class.name, class);

    logln(String::new(), INFO);

    debug_all_capture(c);
}

fn fn_declaration(
    lines: &mut FileReader,
    classes: &mut RefCell<HashMap<&'static str, &'static Class<'static>>>,
    funcs: &mut RefCell<HashMap<&'static str, &'static Func<'static>>>,
    c: Captures,
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

    let name = c.get_str(6).unwrap();

    let return_type_name = c.get_str(1).unwrap();
    let mut return_type: Option<&'static Class<'static>> = None;
    if return_type_name != "void" {
        return_type = classes.get_mut().get(return_type_name.as_str()).copied();
        if return_type.is_none() {
            panic!("return type of func {} not found", name);
        }
    }

    let mut params: Vec<(&Class, &str)> = vec![];

    log(format!("fn {} {}(", return_type_name, name), INFO);

    let m = c.get_str(7);
    if m.is_some() {
        for p in m.unwrap().as_str().split(", ") {
            let param: Vec<&str> = p.trim().split(' ').collect();
            let t = classes.get_mut().get(param[0]);
            if t.is_none() {
                panic!("parameter of func {} not found", name);
            }
            let pname;
            unsafe {
                let i = STRINGS.len();
                STRINGS.push(String::from(param[1]));
                pname = STRINGS[i].as_str();
            }
            params.push((t.unwrap(), pname));
            log(format!("{} {}, ", param[0], pname), INFO);
        }
    }

    let func = add_func(Func {
        name: name.as_str(),
        return_type,
        params: params,
    });

    funcs.get_mut().insert(func.name, func);

    logln(format!(")"), INFO);

    debug_all_capture(c);
}

fn main() -> Result<(), Error> {
    let mut classes: RefCell<HashMap<&str, &Class<'_>>> = RefCell::from(HashMap::from([
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
    let mut funcs: RefCell<HashMap<&str, &Func<'_>>> = RefCell::from(HashMap::new());

    let mut tokens: HashMap<
        &str,
        &dyn Fn(
            &mut FileReader,
            &mut RefCell<HashMap<&'static str, &'static Class<'static>>>,
            &mut RefCell<HashMap<&'static str, &'static Func<'static>>>,
            Captures<'_>,
        ),
    > = HashMap::new();

    tokens.insert(r"^(?'cl'([a-zA-Z]{1}[a-zA-Z0-9]*)(<((?&cl)(, (?&cl))*)>){0,1})(\(((?&cl)(, (?&cl))*)\)){0,1}:$", &class_declaration);
    tokens.insert(r"^(?'cl'([a-zA-Z]{1}[a-zA-Z0-9]*)(<((?&cl)(, (?&cl))*)>){0,1}) ([a-zA-Z]{1}[a-zA-Z0-9]*)\(((?&cl) [a-zA-Z]{1}[a-zA-Z0-9]*(, (?&cl) [a-zA-Z]{1}[a-zA-Z0-9]*)*){0,1}\):$", &fn_declaration);

    let mut lines = FileReader::new("..\\testRedRust\\main.rr").unwrap();

    while lines.current.as_deref().is_some() {
        let line = String::from(lines.current.as_deref().unwrap());
        let mut captured = tokens
            .iter()
            .map(|i| {
                (
                    Regex::new(i.0).unwrap().captures(line.as_bytes()).unwrap(),
                    i.1,
                )
            })
            .filter(|i| i.0.is_some());
        if captured.clone().count() != 1 {
            return Err(Error::new(
                std::io::ErrorKind::NotFound,
                format!("No token match \"{}\"", line),
            ));
        }
        let i = captured.next().unwrap();
        i.1(&mut lines, &mut classes, &mut funcs, i.0.unwrap());
    }

    let mut output = File::create("..\\testC\\testC.c")?;
    write!(output, "...")?;

    Ok(())
}
