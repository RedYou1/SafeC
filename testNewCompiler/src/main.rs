mod _type;
mod captures_utils;
mod class;
mod func;
mod logger;
mod my_file_reader;
mod object;

use _type::Type;
use const_format::formatcp;
use memory_stats::memory_stats;
use once_cell::sync::Lazy;
use pcre2::bytes::Captures;
use pcre2::bytes::Regex;
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
use logger::LogLevel::{ALARM, DEBUG, INFO, WARNING, XDEBUG};
use my_file_reader::FileReader;

fn getmem() {
    if let Some(usage) = memory_stats() {
        logln(
            format!("Current physical memory usage: {}", usage.physical_mem),
            XDEBUG,
        );
        logln(
            format!("Current virtual memory usage: {}", usage.virtual_mem),
            XDEBUG,
        );
    } else {
        logln(
            String::from("Couldn't get the current memory usage :("),
            XDEBUG,
        );
    }
}

static mut CLASSES: Lazy<HashMap<String, Class>> = Lazy::new(|| HashMap::new());
static mut FUNCS: Lazy<HashMap<String, Func>> = Lazy::new(|| HashMap::new());

fn add_class<'a>(c: Class) {
    unsafe {
        CLASSES.insert(c.name.to_string(), c);
    }
}

fn add_class2<'a>(cname: &'static str, c: Class) {
    unsafe {
        CLASSES.insert(cname.to_string(), c);
    }
}

fn add_func<'a>(c: Func) {
    unsafe {
        FUNCS.insert(c.name.to_string(), c);
    }
}

fn add_func2<'a>(cname: &'static str, c: Func) {
    unsafe {
        FUNCS.insert(cname.to_string(), c);
    }
}

fn get_class<'a, 'b>(c: &'a str) -> Option<&'b Class> {
    unsafe {
        return CLASSES.get(c);
    }
}
fn get_func<'a, 'b>(c: &'a str) -> Option<&'b Func> {
    unsafe {
        return FUNCS.get(c);
    }
}

fn get_type<'a>(name: String) -> Type {
    let mut c = name.chars();
    let is_null = name.ends_with('?');
    if is_null {
        c.next_back();
    }
    let is_not_own = name.starts_with('*');
    let is_ref = is_not_own || name.starts_with('&');

    if is_ref {
        c.next();
    }

    let t = get_class(c.as_str());
    if t.is_none() {
        panic!("type {} not found", c.as_str());
    }

    return Type {
        of: t.unwrap(),
        own: !is_not_own,
        is_ref: is_ref,
        nullable: is_null,
        can_be_children: false,
        can_call_func: true,
    };
}

fn class_declaration(lines: &mut FileReader, c: Captures) {
    logln(String::from("entering class_declaration"), XDEBUG);

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

    logln(
        String::from("finished reading content of the class"),
        XDEBUG,
    );

    let name = c.get_str(2).unwrap();
    let mut extends: Option<&Class> = None;
    let mut implements: Vec<&Class> = vec![];

    log(format!("class {}", name), INFO);
    let m = c.get_str(7);
    if m.is_some() {
        let includes: Vec<&str> = m.unwrap().split(", ").collect();

        match get_class(includes[0]) {
            Some(l) => extends = Some(l),
            None => panic!("extends of class {} not found", name),
        }

        log(format!(" extends {}", extends.unwrap().name), INFO);

        if includes.len() > 1 {
            log(format!(" implements "), INFO);
            implements = Vec::with_capacity(includes.len() - 1);
            for i in 1..includes.len() {
                let t = get_class(includes[i]);
                if extends.is_none() {
                    panic!("implements of class {} not found", name);
                }
                implements.push(t.unwrap());
                log(format!("{}, ", t.unwrap().name), INFO);
            }
        }
    }

    add_class(Class {
        name: name.to_string(),
        variables: HashMap::new(),
        extends: extends,
        implements: implements,
    });

    logln(String::new(), INFO);
}

fn fn_declaration(lines: &mut FileReader, c: Captures) {
    logln(String::from("entering fn_declaration"), XDEBUG);

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

    logln(String::from("finished reading content of the func"), XDEBUG);

    let name = c.get_str(8).unwrap();

    let return_type_name = c.get_str(1).unwrap();
    let mut return_type: Option<Type> = None;
    if return_type_name != "void" {
        return_type = Some(get_type(String::from(return_type_name)));
    }

    let mut params: Vec<(Type, String)> = vec![];

    log(format!("fn {} {}(", return_type_name, name), INFO);

    let m = c.get_str(9).unwrap();
    if !m.is_empty() {
        for p in m.split(", ") {
            let param: Vec<&str> = p.trim().split(' ').collect();
            params.push((get_type(String::from(param[0])), String::from(param[1])));
            log(format!("{} {}, ", param[0], param[1]), INFO);
        }
    }

    add_func(Func {
        name: name.to_string(),
        return_type: return_type,
        params: params,
    });

    logln(format!(")"), INFO);
}

const NAMEREGEX: &str = r"(?'nm'[a-zA-Z]{1}[a-zA-Z0-9]*)";
const DEFNAMEREGEX: &str = formatcp!(r"(?'cl'{NAMEREGEX}(<((?&cl)(, (?&cl))*)>){{0,1}})");
const CLASSDEFREGEX: &str = formatcp!(r"^{DEFNAMEREGEX}(\(((?&cl)(, (?&cl))*)\)){{0,1}}:$");
const TYPEREGEX: &str = formatcp!(r"(?'tp'(\*|\&){{0,1}}{DEFNAMEREGEX}\?{{0,1}})");
const FUNCDEFREGEX: &str =
    formatcp!(r"^{TYPEREGEX} ((?&cl))\((((?&tp) (?&nm)(, (?&tp) (?&nm))*){{0,1}})\):$");

fn main() -> Result<(), Error> {
    add_class2(
        "i32",
        Class {
            name: "int".to_string(),
            variables: HashMap::new(),
            extends: None,
            implements: vec![],
        },
    );
    add_class2(
        "f32",
        Class {
            name: "float".to_string(),
            variables: HashMap::new(),
            extends: None,
            implements: vec![],
        },
    );
    add_class2(
        "str",
        Class {
            name: "char*".to_string(),
            variables: HashMap::new(),
            extends: None,
            implements: vec![],
        },
    );

    let mut tokens: HashMap<&str, &dyn Fn(&mut FileReader, Captures<'_>)> = HashMap::new();

    tokens.insert(CLASSDEFREGEX, &class_declaration);
    tokens.insert(FUNCDEFREGEX, &fn_declaration);

    let mut lines = FileReader::new("..\\testRedRust\\main.rr").unwrap();

    getmem();

    while lines.current.as_deref().is_some() {
        let line = String::from(lines.current.as_deref().unwrap());
        logln(format!("Begening test line {}", line.as_str()), DEBUG);
        let mut captured = tokens
            .iter()
            .map(|i| {
                logln(format!("test regex {}", i.0), XDEBUG);
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
        let c = i.0.unwrap();
        debug_all_capture(&c);
        i.1(&mut lines, c);
        getmem();
    }

    let mut output = File::create("..\\testC\\testC.c")?;
    write!(output, "...")?;

    Ok(())
}
