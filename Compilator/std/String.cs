namespace RedRust
{
    internal class String : Array
    {
        public String(Global Global, Type charptr, Type size)
            : base(charptr, size)
        {
            LifeTime current = new();
            functions.Add(
                new Function("StartsWith", Global.u1,
                    new Variable[] { new("this", new Reference(this), current), new("o", new Reference(this), current) }, new(),
                    new Token[]
                    {
                        new FuncLine2("for (int i = 0; i < this->len; i++) {"),
                        new FuncBlock(new Token[]
                        {
                            new FuncLine2("if (i == o->len) {"),
                            new FuncBlock(new Token[]
                            {
                                new FuncLine("return true")
                            }),
                            new FuncLine2("}"),
                            new FuncLine2("if (this->ptr[i] != o->ptr[i]) {"),
                            new FuncBlock(new Token[]
                            {
                                new FuncLine("return false")
                            }),
                            new FuncLine2("}"),
                        }),
                        new FuncLine2("}"),
                        new FuncLine("return false")
                    }));
            functions.Add(
                new Function("EndsWith", Global.u1,
                    new Variable[] { new("this", new Reference(this), current), new("o", new Reference(this), current) }, new(),
                    new Token[]
                    {
                        new FuncLine2("for (int i = 0; i < this->len; i++) {"),
                        new FuncBlock(new Token[]
                        {
                            new FuncLine2("if (i == o->len) {"),
                            new FuncBlock(new Token[]
                            {
                                new FuncLine("return true")
                            }),
                            new FuncLine2("}"),
                            new FuncLine2("if (this->ptr[this->len - i - 1] != o->ptr[i]) {"),
                            new FuncBlock(new Token[]
                            {
                                new FuncLine("return false")
                            }),
                            new FuncLine2("}"),
                        }),
                        new FuncLine2("}"),
                        new FuncLine("return false")
                    }));
        }
    }
}
