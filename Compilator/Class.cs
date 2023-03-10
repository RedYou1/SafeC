namespace RedRust
{
    internal class Class : Pointer, Token
    {
        public readonly string name;
        public readonly Variable[] variables;

        public readonly List<Function> constructs;
        public readonly List<Function> functions;

        public readonly Class? extend;

        public Class(string name, Variable[] variables, Class? extend)
            : base(new(name))
        {
            this.name = name;
            CanDeconstruct = true;
            this.variables = variables;
            this.extend = extend;
            constructs = new List<Function>();
            functions = new List<Function>();
        }

        public void Compile(string tabs, StreamWriter sw)
        {
            sw.WriteLine($$"""{{tabs}}typedef struct {{name}} {""");
            List<string> temp = new();
            var e = extend;
            while (e is not null)
            {
                int i = 0;
                foreach (var v in e.variables)
                {
                    temp.Insert(i, $"{tabs}\t{v.type.id} {v.name};");
                    i++;
                }
                e = e.extend;
            }
            foreach (var v in temp)
            {
                sw.WriteLine(v);
            }
            foreach (var v in variables)
            {
                sw.WriteLine($"{tabs}\t{v.type.id} {v.name};");
            }
            sw.WriteLine($$"""{{tabs}}}{{name}};""");

            foreach (var v in constructs)
            {
                v.Compile(tabs, sw);
            }

            sw.WriteLine($"{tabs}void {name}_DeConstruct({id} this) {{");
            foreach (var line in variables)
            {
                if (!line.toDelete)
                    continue;
                if (line.type is Class _class)
                    sw.WriteLine($"{tabs}\t{_class.name}_DeConstruct(this->{line.name});");
                else if (line.type is Pointer)
                    sw.WriteLine($"{tabs}\tfree(this->{line.name});");
            }
            sw.WriteLine($"{tabs}\tfree(this);");
            sw.WriteLine($"{tabs}}}");

            foreach (var v in functions)
            {
                v.Compile(tabs, sw);
            }
            foreach (var v in implicitCast)
            {
                v.convert?.Compile(tabs, sw);
            }
            foreach (var v in explicitCast)
            {
                v.converter.Compile(tabs, sw);
            }
        }

        internal class Free : Token
        {
            private string name;

            public Free(string name)
            {
                this.name = name;
            }

            public void Compile(string tabs, StreamWriter sw)
            {
                sw.WriteLine($"{tabs}free({name});");
            }
        }
    }
}
