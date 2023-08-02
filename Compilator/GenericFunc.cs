namespace RedRust
{
	public class GenericFunc : IFunc
	{
		public string Name { get; }

		public readonly Func<Dictionary<string, Class>, Type>[] Parameters;
		public readonly Dictionary<Dictionary<string, Class>, Func> Funcs;
		public readonly Func<Dictionary<string, Class>, Func> FuncGenerator;
		public GenericFunc(string name, Func<Dictionary<string, Class>, Type>[] parameters, Func<Dictionary<string, Class>, Func> funcGenerator)
		{
			Name = name;
			Parameters = parameters;
			Funcs = new();
			FuncGenerator = funcGenerator;
		}

		private Func? getFunc(Dictionary<string, Class> g)
		{
			foreach (var kvp in Funcs)
			{
				foreach (var kvp2 in kvp.Key)
					if (g.TryGetValue(kvp2.Key, out Class? c) &&
						c is not null &&
						c.Equals(kvp2.Value))
						return kvp.Value;
			}
			return null;
		}

		public IFunc.CanCallReturn? CanCall(Func from, Dictionary<string, Class>? gen, params Action[] args)
		{
			if (Parameters.Length != args.Length || gen is null)
				return null;

			Func? t = getFunc(gen);
			if (t is not null)
				return t.CanCall(from, gen, args);

			t = FuncGenerator(gen);
			Funcs.Add(gen, t);
			return t.CanCall(from, gen, args);
		}

		public IEnumerable<string> Compile()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Token> ToInclude()
		{
			throw new NotImplementedException();
		}
	}
}
