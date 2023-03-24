namespace RedRust
{
    internal class Variable
    {
        public readonly string name;
        public readonly Type type;
        public LifeTime lifeTime;

        public Variable(string name, Type type, LifeTime lifeTime)
        {
            this.name = name;
            this.type = type;
            this.lifeTime = lifeTime;
        }

        public void DeleteVar(LifeTime current, List<Token> lines, string? line)
        {
            if (type.isReference())
                return;
            if (lifeTime != current)
                return;
            if (!type.CanDeconstruct)
                return;

            string pre = string.Empty;
            var _class = type.AsClass();

            if (_class is null)
                return;

            if (line is null || !name.Equals(line.Substring(0, line.Length - 1)))
            {
                var t = type;
                while (t is Modifier mod)
                {
                    if (mod is Nullable n)
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
