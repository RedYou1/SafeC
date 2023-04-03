namespace RedRust
{
    internal class VariableManager
    {
        private readonly Stack<List<string>> names;
        private readonly Stack<List<Variable>> variables;

        public IEnumerable<string> Names => names.SelectMany(l => l);
        public IEnumerable<Variable> Variables => variables.SelectMany(l => l);

        public List<Variable> Peek => variables.Peek();

        public VariableManager()
        {
            names = new Stack<List<string>>();
            variables = new Stack<List<Variable>>();
            Push();
        }

        public Variable? GetName(string name, LifeTime current)
            => GetFunc(v => v.name.Equals(name), current);

        public bool GetName(string name, LifeTime current, out Variable? v)
            => GetFunc(v => v.name.Equals(name), current, out v);

        public Variable? GetFunc(Func<Variable, bool> func, LifeTime current)
        {
            GetFunc(func, current, out var v);
            return v;
        }

        public bool GetFunc(Func<Variable, bool> func, LifeTime current, out Variable? v)
        {
            v = Variables.FirstOrDefault(v => v.VariableAction is OwnedVariable o && current.Ok(o.lifeTime) && func(v));
            return v is not null;
        }

        public Variable Add(string name, Func<string, Variable> var)
        {
            if (!Names.Contains(name))
            {
                names.Peek().Add(name);
                var v = var(name);
                variables.Peek().Add(v);
                return v;
            }

            int i = 2;
            while (true)
            {
                string newName = $"{name}{i}";
                if (!Names.Contains(newName))
                {
                    names.Peek().Add(newName);
                    var v = var(newName);
                    variables.Peek().Add(v);
                    return v;
                }
                i++;
            }
        }

        public void Push()
        {
            names.Push(new());
            variables.Push(new());
        }

        public void Pop()
        {
            names.Pop();
            variables.Pop();
        }
    }
}
