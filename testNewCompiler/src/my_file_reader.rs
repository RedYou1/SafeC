use std::fs::File;
use std::io::{BufRead, BufReader, Error};

pub struct FileReader {
    stream: std::io::Lines<BufReader<File>>,
    pub current: Box<Option<String>>,
    pub next: Box<Option<String>>,
}

fn extract(arg: Option<Result<String, Error>>) -> Option<String> {
    match arg {
        Some(l) => match l {
            Ok(a) => return Option::Some(a),
            Err(_) => panic!("Read file error"),
        },
        None => return Option::None,
    }
}

impl FileReader {
    pub fn new(path: &str) -> Result<FileReader, Error> {
        let mut fr = FileReader {
            stream: BufReader::new(File::open(path)?).lines().into_iter(),
            current: Box::new(None),
            next: Box::new(None),
        };

        fr.current = Box::new(extract(fr.stream.next()));
        fr.next = Box::new(extract(fr.stream.next()));

        return Result::Ok(fr);
    }

    pub fn next(&mut self) {
        std::mem::swap(&mut self.current, &mut self.next);
        self.next = Box::new(extract(self.stream.next()));
    }
}
