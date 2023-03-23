namespace RedRust
{
    internal class Variable
    {
        public readonly string name;
        public readonly Type type;
        public bool toDelete;

        public Variable(string name, Type type, bool toDelete)
        {
            this.name = name;
            this.type = type;
            this.toDelete = toDelete;
        }

        public void DeleteVar(List<Token> lines, string? line)
        {
            string pre = string.Empty;
            var _class = type.AsClass();

            if (toDelete && _class is not null && (line is null || !name.Equals(line.Substring(0, line.Length - 1))))
            {
                var t = type;
                while (t is Modifier mod)
                {
                    if (mod is RedRust.Nullable n)
                    {
                        if (n.isNull)
                        {
                            lines.Add(new FuncLine2($"if ({name} != NULL) {{"));
                            pre = "\t";
                        }
                        break;
                    }
                    t = mod.of;
                }

                lines.Add(new FuncLine($"{pre}{_class.name}_DeConstruct({name})"));

                if (t is RedRust.Nullable n2)
                    if (n2.isNull)
                        lines.Add(new FuncLine2($"}}"));
            }
        }
    }
}
