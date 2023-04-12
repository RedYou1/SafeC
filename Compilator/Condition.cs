namespace RedRust
{
    internal class Condition : Type
    {
        public Condition(Type u1) : base(u1.id) { }

        public virtual void InIf(List<Token> lines, VariableManager variables, LifeTime current) { }
        public virtual void InElse(List<Token> lines, VariableManager variables, LifeTime current) { }
        public virtual void AfterIf(List<Token> lines, VariableManager variables, LifeTime current) { }
    }

    internal class NullCondition : Condition
    {
        private readonly Nullable CurrentType;
        public NullCondition(Type u1, Nullable currentType) : base(u1)
        {
            CurrentType = currentType;
            if (!CurrentType.isNull)
                throw new Exception("Cant access code");
        }

        public override void InIf(List<Token> lines, VariableManager variables, LifeTime current) { }
        public override void InElse(List<Token> lines, VariableManager variables, LifeTime current)
        {
            CurrentType.isNull = false;
        }
        public override void AfterIf(List<Token> lines, VariableManager variables, LifeTime current)
        {
            CurrentType.isNull = true;
        }
    }

    internal class NotNullCondition : Condition
    {
        private readonly Nullable CurrentType;
        public NotNullCondition(Type u1, Nullable currentType) : base(u1)
        {
            CurrentType = currentType;
            if (!CurrentType.isNull)
                throw new Exception("Useless if");
        }

        public override void InIf(List<Token> lines, VariableManager variables, LifeTime current)
        {
            CurrentType.isNull = false;
        }
        public override void InElse(List<Token> lines, VariableManager variables, LifeTime current)
        {
            CurrentType.isNull = true;
        }
        public override void AfterIf(List<Token> lines, VariableManager variables, LifeTime current)
        {
            CurrentType.isNull = true;
        }
    }

    internal class TypeCondition : Condition
    {
        private readonly string VarName;
        private readonly Typed CurrentType;
        private readonly Type Cast;
        private readonly string? Name;

        public TypeCondition(Type u1, string varName, Typed currentType, Type cast, string? name) : base(u1)
        {
            VarName = varName;
            CurrentType = currentType;
            Cast = cast;
            Name = name;
        }

        public override void InIf(List<Token> lines, VariableManager variables, LifeTime current)
        {
            if (Name is null)
                return;
            var v = variables.Add(Name, (name) => new(name, Cast, current));
            v.VariableAction = new RefOwnedVariable(v, current);
            lines.Add(new FuncLine($"{Cast.id} {v.name} = {VarName}->ptr"));
        }
        public override void InElse(List<Token> lines, VariableManager variables, LifeTime current) { }
        public override void AfterIf(List<Token> lines, VariableManager variables, LifeTime current) { }
    }

    internal class NotTypeCondition : Condition
    {
        private readonly string VarName;
        private readonly Typed CurrentType;
        private readonly Type Cast;
        private readonly string? Name;

        public NotTypeCondition(Type u1, string varName, Typed currentType, Type cast, string? name) : base(u1)
        {
            VarName = varName;
            CurrentType = currentType;
            Cast = cast;
            Name = name;
        }

        public override void InIf(List<Token> lines, VariableManager variables, LifeTime current) { }
        public override void InElse(List<Token> lines, VariableManager variables, LifeTime current) { }
        public override void AfterIf(List<Token> lines, VariableManager variables, LifeTime current)
        {
            if (Name is null)
                return;
            var v = variables.Add(Name, (name) => new(name, Cast, current));
            v.VariableAction = new RefOwnedVariable(v, current);
            lines.Add(new FuncLine($"{Cast.id} {v.name} = {VarName}->ptr"));
        }
    }
}
