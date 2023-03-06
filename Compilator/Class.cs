namespace RedRust
{
    internal class Class : Type, Token
    {
        public readonly (Type type, string name)[] variables;

        public readonly List<Function> constructs;
        public readonly Function deConstruct;
        public readonly List<Function> functions;

        public readonly Class? extend;

        public Class(string name, (Type type, string name)[] variables, Class? extend, Function deConstruct)
            : base(name)
        {
            CanDeconstruct = true;
            this.variables = variables;
            this.extend = extend;
            constructs = new List<Function>();
            functions = new List<Function>();
            this.deConstruct = deConstruct;
        }

        public void Compile(string tabs, StreamWriter sw)
        {
            sw.WriteLine($$"""{{tabs}}typedef struct {""");
            List<string> temp = new();
            var e = extend;
            while (e is not null)
            {
                int i = 0;
                foreach (var v in e.variables)
                {
                    temp.Insert(i, $"{tabs}\t{v.type.name} {v.name};");
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
                sw.WriteLine($"{tabs}\t{v.type.name} {v.name};");
            }
            sw.WriteLine($$"""{{tabs}}}{{name}};""");

            foreach (var v in constructs!)
            {
                v.Compile(tabs, sw);
            }
            deConstruct!.Compile(tabs, sw);
            foreach (var v in functions!)
            {
                v.Compile(tabs, sw);
            }
        }

        internal class Malloc : Token
        {
            private string name;

            public Malloc(string name)
            {
                this.name = name;
            }

            public void Compile(string tabs, StreamWriter sw)
            {
                sw.WriteLine($"{tabs}return ({name}*)malloc(sizeof({name}));");
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
