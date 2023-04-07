namespace RedRust
{
    internal class String : Array
    {
        public String(Global Global, Type charptr, Type size)
            : base(charptr, size)
        {
            LifeTime current = new();
            functions.Add(
                new Function($"{name}_StartsWith", Global.u1,
                    new Variable[] { new("this", new Reference(this), current), new("o", new Reference(this), current) }, new(),
                    new Token[]
                    {
                        new FuncLine($"return o->len > this->len ? false : memcmp(this->ptr, o->ptr, o->len) == 0")
                    }));
            functions.Add(
                new Function($"{name}_EndsWith", Global.u1,
                    new Variable[] { new("this", new Reference(this), current), new("o", new Reference(this), current) }, new(),
                    new Token[]
                    {
                        new FuncLine($"return o->len > this->len ? false : memcmp(&this->ptr[this->len - o->len - 1], o->ptr, o->len) == 0")
                    }));
        }
    }
}
