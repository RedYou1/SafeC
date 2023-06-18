use pcre2::bytes::Captures;

pub trait CapturesGetStr {
    fn get_str(&self, i: usize) -> Option<&str>;
}

impl<'a> CapturesGetStr for Captures<'a> {
    fn get_str(&self, i: usize) -> Option<&str> {
        let a = self.get(i);
        if a.is_none() {
            return None;
        }
        let b = std::str::from_utf8(a.unwrap().as_bytes());
        if b.is_err() {
            panic!("{}", b.unwrap_err());
        }
        return Some(b.unwrap());
    }
}
