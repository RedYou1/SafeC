using PCRE;

namespace RedRust
{
	public class Class : IClass, IEquatable<Class>
	{
		protected bool Included;

		public string Name { get; }

		public Class? Extends { get; set; }
		public Class[] Implements { get; set; }

		public Dictionary<Class, Func<Action, Action>> Casts = new();

		public readonly List<Declaration> Variables = new();
		public readonly Dictionary<string, IFunc> Funcs = new();
		public readonly List<IFunc> Constructors = new();

		public IEnumerable<Declaration> AllVariables
		{
			get
			{
				if (Extends is not null)
					foreach (var v in Extends.AllVariables)
						yield return v;
				foreach (var v in Variables)
					yield return v;
			}
		}

		private IEnumerable<(string, IFunc)> AllFuncs_Enum
		{
			get
			{
				foreach (var f in Funcs)
					yield return (f.Key, f.Value);
				if (Extends is not null)
					foreach (var f in Extends.AllFuncs_Enum)
						yield return f;
			}
		}
		public Dictionary<string, IFunc> AllFuncs => AllFuncs_Enum
			.GroupBy(p => p.Item1, StringComparer.OrdinalIgnoreCase)
			.ToDictionary(g => g.Key, g => g.First().Item2, StringComparer.OrdinalIgnoreCase);

		public List<IClass> Childs { get; } = new();

		public Class(string name, Class? extends, Class[] implements, bool included = false)
		{
			Name = name;
			Extends = extends;
			Implements = implements;
			Included = included;

			if (!included && !Name.Equals("void") && !Name.Equals("Classes"))
				Program.Classes.Options.Add(Name);
			extends?.Childs.Add(this);
		}

		public static IClass Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromC is not null || fromF is not null || from.Any())
				throw new Exception();

			string[] m = captures[7].Value.Trim().Split(", ");
			string name = captures[2];
			string gens = captures[4];

			IClass c;

			if (string.IsNullOrEmpty(gens))
			{
				c = new Class(
					captures[2],
					string.IsNullOrWhiteSpace(m[0]) ? null : IClass.IsClass(Program.GetClass(m[0], gen)),
					m.Skip(1).Select(Program.GetInterface).ToArray());
			}
			else
			{
				c = new GenericClass(name, gens.Split(", "), null, Array.Empty<Class>());
			}

			Program.Tokens.Add(c.Name, c);

			var fr = lines.Extract();

			IEnumerable<Declaration> Implement(Class cc, Dictionary<string, Class>? gen)
			{
				fr.Line = 0;
				foreach (var t in fr.Parse(cc, null, gen, Array.Empty<Token>()))
				{
					if (t is Declaration d)
						yield return d;
				}
			}

			if (c is Class cc)
				foreach (Declaration d in Implement(cc, null))
					cc.Variables.Add(d);
			else if (c is GenericClass gc)
				gc.Variables = Implement;
			else
				throw new Exception();



			return c;
		}

		public IEnumerable<Token> ToInclude()
		{
			if (Extends is not null)
				yield return Extends;
			foreach (var i in Implements)
				yield return i;
			foreach (var v in Variables)
				foreach (var vv in v.ToInclude())
					yield return vv;
		}

		public virtual IEnumerable<string> Compile()
		{
			if (Included)
				yield break;
			Included = true;

			if (AllVariables.Any())
			{
				yield return $"typedef struct {Name} {{";
				foreach (var v in AllVariables)
					yield return $"\t{v.ReturnType} {v.Name}";
				yield return $"}}{Name}";
			}
			else
			{
				yield return $"typedef struct {Name} {Name}";
			}
		}

		public static IFunc ConstructorDeclaration(FileReader lines, PcreMatch captures, IClass? fromIC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromIC is not Class fromC)
				throw new Exception();

			string name = captures[2];
			string gens = captures[4];
			string[] _params = captures[6].Value.Split(", ");

			var fr = lines.Extract();
			int id = fromC.Constructors.Count / 2;

			//Base
			string fname = $"{fromC.Name}_Base_{name}_{id}";
			Type type = new Type(fromC, false, true, false, true, false);
			IFunc f;
			if (string.IsNullOrEmpty(gens))
			{
				f = new Func(new Type(Program.VOID, false, false, false, false, false),
					(string.IsNullOrWhiteSpace(_params[0]) ? Array.Empty<Parameter>()
						: _params.Select(p =>
						{
							string[] p2 = p.Split(" ");
							return new Parameter(Program.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen), p2.Last());
						})).Prepend(new Parameter(type, "this")).ToArray())
				{
					Name = fname
				};

				foreach (var t in fr.Parse(fromC, (Func)f, gen, from))
				{
					if (t is not Action a)
						throw new Exception();
					((Func)f).Actions.Add(a);
				}
				fr.Line = 0;
			}
			else
				f = new GenericFunc(fromC, fname, (string.IsNullOrWhiteSpace(_params[0]) ? 0 : _params.Length) + 1,
					gen, gens.Split(", "),
					_ =>
					{
						fr.Line = 0;
						return fr;
					},
					_ => type,
					gen2 => _params.Select(p =>
					{
						string[] p2 = p.Split(" ");
						return new Parameter(Program.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen2), p2.Last());
					}).Prepend(new Parameter(type, "this")).ToArray());

			fromC.Constructors.Add(f);


			//New
			fname = $"{fromC.Name}_{name}_{id}";
			type = new Type(fromC, true, false, false, false, true);
			if (string.IsNullOrEmpty(gens))
			{
				f = new Func(
					type,
					string.IsNullOrWhiteSpace(_params[0]) ? Array.Empty<Parameter>()
						: _params.Select(p =>
						{
							string[] p2 = p.Split(" ");
							return new Parameter(Program.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen), p2.Last());
						}).ToArray())
				{
					Name = fname
				};
				((Func)f).Objects.Add("this", new(type, "this"));

				((Func)f).Actions.Add(new Declaration(type, null) { Name = "this" });
				foreach (var t in fr.Parse(fromC, (Func)f, gen, from))
				{
					if (t is not Action a)
						throw new Exception();
					((Func)f).Actions.Add(a);
				}
				((Func)f).Actions.Add(new Return(((Func)f).Objects["this"]));
			}
			else
			{
				FileReader fr2 = new(fr.Lines.Prepend(new($"{fromC.Name} this", func => func.Objects.Add("this", new(type, "this")))).Append("return this").ToArray());
				f = new GenericFunc(fromC, fname, string.IsNullOrWhiteSpace(_params[0]) ? 0 : _params.Length,
					gen, gens.Split(", "),
					_ =>
					{
						fr2.Line = 0;
						return fr2;
					},
					_ => type,
					gen2 => _params.Select(p =>
					{
						string[] p2 = p.Split(" ");
						return new Parameter(Program.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen2), p2.Last());
					}).ToArray());
			}

			fromC.Constructors.Add(f);

			return f;
		}

		public bool Equals(Class? other)
			=> other is not null && Name.Equals(other.Name);
	}
}
