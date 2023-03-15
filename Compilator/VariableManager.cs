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

        public Variable? GetName(string name)
        => Variables.FirstOrDefault(v => v.name.Equals(name));

        public bool GetName(string name, out Variable? v)
        {
            v = Variables.FirstOrDefault(v => v.name.Equals(name));
            return v is not null;
        }

        public Variable? GetFunc(Func<Variable, bool> func)
        => Variables.FirstOrDefault(func);

        public bool GetFunc(Func<Variable, bool> func, out Variable? v)
        {
            v = Variables.FirstOrDefault(func);
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
