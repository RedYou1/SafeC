namespace RedRust
{
    internal class Function : Token
    {
        public readonly Type returnType;
        public readonly string name;
        public readonly Variable[] parameters;
        public readonly Token[] lines;

        public Function(string name, Type returnType, Variable[] parameters, params Token[] lines)
        {
            this.name = name;
            this.returnType = returnType;
            this.parameters = parameters;
            this.lines = lines;
        }

        public (Function converter, bool toDelete)?[]? CanExecute(Type[] types)
        {
            if (parameters.Length != types.Length)
                return null;
            (Function converter, bool toDelete)?[] functions = new (Function converter, bool toDelete)?[types.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var function = parameters[i].type.Equivalent(types[i]);
                if (!function.success)
                    return null;
                functions[i] = function._explicit;
            }
            return functions;
        }

        public void Compile(string tabs, StreamWriter sw)
        {
            sw.Write($"{tabs}{returnType.id} {name}(");
            if (parameters.Any())
            {
                sw.Write($"{parameters.First().type.id} {parameters.First().name}");
                foreach (var parameter in parameters.Skip(1))
                {
                    sw.Write($", {parameter.type.id} {parameter.name}");
                }
            }
            sw.WriteLine(") {");
            foreach (var line in lines)
            {
                line.Compile($"{tabs}\t", sw);
            }
            sw.WriteLine($$"""{{tabs}}}""");
        }
    }
}
