namespace RedRust
{
    internal class Function : Token
    {
        public readonly Type returnType;
        public readonly string name;
        public readonly (Type type, string name)[] parameters;
        public readonly Token[] lines;

        public Function(string name, Type returnType, (Type type, string name)[] parameters, params Token[] lines)
        {
            this.name = name;
            this.returnType = returnType;
            this.parameters = parameters;
            this.lines = lines;
        }

        public bool CanExecute(Type[] types)
        {
            if (parameters.Length != types.Length)
                return false;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].type.Equivalent(types[i]))
                    return false;
            }
            return true;
        }

        public void Compile(string tabs, StreamWriter sw)
        {
            sw.Write($"{tabs}{returnType.name} {name}(");
            if (parameters.Any())
            {
                sw.Write($"{parameters.First().type.name}");
                if (parameters.First().type is Class)
                    sw.Write("*");
                sw.Write($" {parameters.First().name}");
                foreach (var parameter in parameters.Skip(1))
                {
                    sw.Write($", {parameter.type.name} {parameter.name}");
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
