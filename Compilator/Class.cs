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

			IClass c;

			if (name.EndsWith('>'))
			{
				var i = name.IndexOf('<');
				c = new GenericClass(name.Substring(0, i), name.Substring(i + 1, name.Length - i - 2).Split(", "), null, Array.Empty<Class>());
			}
			else
			{
				c = new Class(
					captures[2],
					string.IsNullOrWhiteSpace(m[0]) ? null : IClass.IsClass(Program.GetClass(m[0], gen)),
					m.Skip(1).Select(Program.GetInterface).ToArray());
			}

			Program.Tokens.Add(c.Name, c);

			var fr = lines.Extract();

			IEnumerable<Declaration> Implement(Class cc, Dictionary<string, Class>? gen)
			{
				fr.Reset();
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

		public static Func ConstructorDeclaration(FileReader lines, PcreMatch captures, IClass? fromIC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromIC is not Class fromC)
				throw new Exception();

			string name = captures[1];
			string _params = captures[3];

			var fr = lines.Extract();
			int id = fromC.Constructors.Count / 2;

			//Base
			Type type = new Type(fromC, false, true, false, true, false);
			var f = new Func(
				new Type(Program.VOID, false, false, false, false, false),
				(string.IsNullOrWhiteSpace(_params) ? Array.Empty<Parameter>()
					: _params.Split(", ").Select(p =>
					{
						string[] p2 = p.Split(" ");
						return new Parameter(Program.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen), p2.Last());
					})).Prepend(new Parameter(type, "this")).ToArray())
			{
				Name = $"{fromC.Name}_Base_{name}_{id}"
			};
			fromC.Constructors.Add(f);

			foreach (var t in fr.Parse(fromC, f, gen, from))
			{
				if (t is not Action a)
					throw new Exception();
				f.Actions.Add(a);
			}

			fr.Reset();

			//New
			type = new Type(fromC, true, false, false, false, false);
			f = new Func(
				type,
				string.IsNullOrWhiteSpace(_params) ? Array.Empty<Parameter>()
					: _params.Split(", ").Select(p =>
					{
						string[] p2 = p.Split(" ");
						return new Parameter(Program.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen), p2.Last());
					}).ToArray())
			{
				Name = $"{fromC.Name}_{name}_{id}"
			};
			f.Objects.Add("this", new(type, "this"));

			fromC.Constructors.Add(f);

			f.Actions.Add(new Declaration(type, null) { Name = "this" });

			foreach (var t in fr.Parse(fromC, f, gen, from))
			{
				if (t is not Action a)
					throw new Exception();
				f.Actions.Add(a);
			}

			f.Actions.Add(new Return(f.Objects["this"]));

			return f;
		}

		public bool Equals(Class? other)
			=> other is not null && Name.Equals(other.Name);
	}
}
