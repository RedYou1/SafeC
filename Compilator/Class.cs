namespace RedRust
{
    internal class Class : Pointer, Incluer
    {
        public List<Includable> includes { get; }
        public bool included { get; set; } = false;

        public readonly string name;
        public readonly Variable[] variables;
        public readonly Variable[] allVariables;

        public readonly List<Function> constructs;
        public readonly List<Function> functions;

        public readonly Class? extend;
        public List<Class> inherit = new();
        public Typed? typed = null;

        public Class(string name, Variable[] variables, Class? extend)
            : base(new(name))
        {
            this.name = name;
            CanDeconstruct = true;
            this.variables = variables;

            List<Variable> allVariables = new();

            if (extend is not null)
                allVariables.AddRange(extend.allVariables);

            allVariables.AddRange(variables);

            this.allVariables = allVariables.ToArray();

            this.extend = extend;
            constructs = new List<Function>();
            functions = new List<Function>();

            includes = allVariables.Select(v => v.type is Includable i ? i : null)
                .Where(v => v is not null).Cast<Includable>().ToList();
        }

        public virtual List<ToCallFunc> GetFunctions(string funcName, (Type type, LifeTime lifeTime)[] args, LifeTime current)
        {
            if (functions is not null)
            {
                List<Converter>[]? converts = null;
                Function? func = functions.FirstOrDefault(f => f.name.StartsWith($"{name}_{funcName}") && (converts = f.CanExecute(args)) is not null);
                if (func is not null && converts is not null)
                    return new() { new(this, func, converts) };
            }
            return extend?.GetFunctions(funcName, args, current) ?? throw new Exception("Function not found");
        }

        public IEnumerable<Class> inherits()
        {
            foreach (var c in inherit)
                yield return c;
            foreach (var c in inherit)
                foreach (var c2 in c.inherits())
                    yield return c2;
        }

        public virtual void Compile(string tabs, StreamWriter sw)
        {
            if (included)
                return;
            included = true;

            foreach (var include in includes)
                include.Compile(tabs, sw);

            sw.WriteLine($$"""{{tabs}}typedef struct {{name}} {""");
            foreach (var v in allVariables)
                sw.WriteLine($"{tabs}\t{v.type.id} {v.name};");
            sw.WriteLine($$"""{{tabs}}}{{name}};""");

            sw.WriteLine($"{tabs}void {name}_DeConstruct({id} this) {{");
            foreach (Variable line in variables)
            {
                if (line.type.isReference())
                    continue;
                if (line.type is Class _class)
                    sw.WriteLine($"{tabs}\t{_class.name}_DeConstruct(this->{line.name});");
                else if (line.type is Pointer)
                    sw.WriteLine($"{tabs}\tfree(this->{line.name});");
            }
            sw.WriteLine($"{tabs}\tfree(this);");
            sw.WriteLine($"{tabs}}}");

            if (typed is not null)
            {
                typed.Compile(tabs, sw);
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
