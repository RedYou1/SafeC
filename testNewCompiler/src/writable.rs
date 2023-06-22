pub trait Writable {
    unsafe fn write(&mut self) -> String;
}
