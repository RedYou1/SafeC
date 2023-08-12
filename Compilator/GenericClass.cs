namespace SafeC
{
	internal class GenericClass : Generic<Class>, IClass
	{
		public string Name { get; }
		public string[] GenNames { get; }
		public Class? Extends { get; set; }
		public Class[] Implements { get; set; }

		public Func<Class, Dictionary<string, Class>, IEnumerable<ActionContainer>> Variables = null!;
		public List<Func<Class, Dictionary<string, Class>, IFunc>> Constructors = new();
		public List<Func<Class, Dictionary<string, Class>, (Class, Func<Object, Object>)>> Casts = new();
		public List<Func<Class, Dictionary<string, Class>, IFunc>> Funcs = new();

		public List<IClass> Childs { get; } = new();

		public GenericClass(string name, string[] genNames, Class? extends, Class[] implements)
		{
			Name = name;
			GenNames = genNames;
			Extends = extends;
			Implements = implements;

			extends?.Childs.Add(this);
		}

		public Class GenerateClass(Class[] gen, out Dictionary<string, Class> ogen)
		{
			if (gen.Length != GenNames.Length)
				throw new Exception();

			ogen = new();
			for (int i = 0; i < GenNames.Length; i++)
				ogen.Add(GenNames[i], gen[i]);

			Class? t = GetObject(gen);
			if (t is not null)
				return t;

			Class c = new Class($"{Name}${string.Join('$', gen.Select(g => g.Name))}", null, Array.Empty<Class>());
			Objects.Add(gen, c);

			foreach (ActionContainer d in Variables(c, ogen))
				c.Variables.Add(d);

			foreach (Func<Class, Dictionary<string, Class>, IFunc> f in Constructors)
				c.Constructors.Add(f(c, ogen));

			foreach (Func<Class, Dictionary<string, Class>, (Class, Func<Object, Object>)> f in Casts)
			{
				var ff = f(c, ogen);
				c.Casts.Add(ff.Item1, ff.Item2);
			}

			foreach (Func<Class, Dictionary<string, Class>, IFunc> f in Funcs)
			{
				var ff = f(c, ogen);
				c.Funcs.Add(ff.Name, ff);
			}

			return c;
		}

		public IEnumerable<Token> ToInclude()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> Compile()
		{
			throw new NotImplementedException();
		}
	}
}
