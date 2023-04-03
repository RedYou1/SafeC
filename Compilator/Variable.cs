namespace RedRust
{
    internal interface VariableAction
    {
        public void DeleteVar(LifeTime current, List<Token> lines, string? line);
    }

    internal class Variable
    {
        public readonly string name;
        public readonly Type type;
        public VariableAction VariableAction;

        public LifeTime? lifeTime => VariableAction is OwnedVariable o ? o.lifeTime : null;

        public Variable(string name, Type type, LifeTime? current)
        {
            this.name = name;
            this.type = type;
            if (current is not null)
                VariableAction = new OwnedVariable(this, current);
            else
                VariableAction = new DeadVariable();
        }

        public void DeleteVar(LifeTime current, List<Token> lines, string? line)
        {
            VariableAction.DeleteVar(current, lines, line);
        }
    }

    internal class DeadVariable : VariableAction
    {
        public DeadVariable() { }

        public void DeleteVar(LifeTime current, List<Token> lines, string? line) { }
    }

    internal class OwnedVariable : VariableAction
    {
        public readonly Variable parent;
        public LifeTime lifeTime;

        public OwnedVariable(Variable parent, LifeTime lifeTime)
        {
            this.parent = parent;
            this.lifeTime = lifeTime;
        }

        public void DeleteVar(LifeTime current, List<Token> lines, string? line)
        {
            if (parent.type.isReference())
                return;
            if (lifeTime != current)
                return;
            if (!parent.type.CanDeconstruct)
                return;

            string pre = string.Empty;
            var _class = parent.type.AsClass();

            if (_class is null)
                return;

            if (line is null || !parent.name.Equals(line.Substring(0, line.Length - 1)))
            {
                var t = parent.type;
                while (t is Modifier mod)
                {
                    if (mod is Nullable n)
                    {
                        if (n.isNull)
                        {
                            lines.Add(new FuncLine2($"if ({parent.name} != NULL) {{"));
                            pre = "\t";
                        }
                        break;
                    }
                    t = mod.of;
                }

                lines.Add(new FuncLine($"{pre}{_class.name}_DeConstruct({parent.name})"));

                if (t is Nullable n2)
                    if (n2.isNull)
                        lines.Add(new FuncLine2($"}}"));

                parent.VariableAction = new DeadVariable();
            }
        }
    }
}
