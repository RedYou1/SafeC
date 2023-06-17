use pcre2::bytes::Captures;

pub trait CapturesGetStr {
    fn get_str(&self, i: usize) -> Option<String>;
}

impl<'a> CapturesGetStr for Captures<'a> {
    fn get_str(&self, i: usize) -> Option<String> {
        let a = self.get(i);
        if a.is_none() {
            return None;
        }
        let b = String::from_utf8(a.unwrap().as_bytes().to_vec());
        if b.is_err() {
            return None;
        }
        return Some(b.unwrap());
    }
}
