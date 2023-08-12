namespace SafeC
{
	internal class GenericFunc : Generic<Func>, IFunc
	{
		public string Name { get; }
		public readonly IClass? Class;

		public readonly string[] GenNames;
		public readonly int ParametersLen;
		public readonly Dictionary<string, Class>? Gen;
		public readonly Func<Dictionary<string, Class>, FileReader> FileReader;
		public readonly Func<Dictionary<string, Class>, Type> ReturnType;
		public readonly Func<Dictionary<string, Class>, Parameter[]> Parameters;

		public GenericFunc(IClass? @class, string name, int parametersLen,
			Dictionary<string, Class>? gen,
			string[] genNames,
			Func<Dictionary<string, Class>, FileReader> fileReader,
			Func<Dictionary<string, Class>, Type> returnType,
			Func<Dictionary<string, Class>, Parameter[]> parameters)
		{
			Class = @class;
			Name = name;
			ParametersLen = parametersLen;
			Gen = gen;
			GenNames = genNames;
			FileReader = fileReader;
			ReturnType = returnType;
			Parameters = parameters;
		}

		public IFunc.CanCallReturn? CanCall(Func from, Dictionary<string, Class>? gen, params Action[] args)
		{
			if (ParametersLen != args.Length || gen is null)
				return null;

			Class[] classes = new Class[GenNames.Length];
			for (int i = 0; i < GenNames.Length; i++)
				if (gen.TryGetValue(GenNames[i], out Class? cls) && cls is not null)
					classes[i] = cls;
				else
					throw new Exception();

			Func? t = GetObject(classes);
			if (t is not null)
				return t.CanCall(from, gen, args);


			Dictionary<string, Class> gen3 = new(gen);
			if (Gen is not null)
				foreach (var g in Gen)
					gen3.Add(g.Key, g.Value);

			var @return = ReturnType(gen3);
			var @params = Parameters(gen3);


			t = new Func(@return, @params)
			{ Name = $"{Name}${string.Join('$', gen.Select(g => g.Value.Name))}" };

			foreach (Token ta in FileReader(gen3).Parse(Class, t, gen, Array.Empty<Token>()))
			{
				if (ta is not ActionContainer a)
					throw new Exception();
				t.Actions.Add(a);
			}

			Objects.Add(classes, t);
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
