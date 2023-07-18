using PCRE;

namespace RedRust
{
	public class Class : Token, IEquatable<Class>
	{
		protected bool Included;

		public string Name { get; }

		public Class? Extends;
		public Class[] Implements;

		public Dictionary<Class, Func<Action, Action>> Casts = new();

		public readonly List<Declaration> Variables = new();
		public readonly Dictionary<string, Func> Funcs = new();
		public readonly List<Func> Constructors = new();

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

		private IEnumerable<(string, Func)> AllFuncs_Enum
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
		public Dictionary<string, Func> AllFuncs => AllFuncs_Enum
			.GroupBy(p => p.Item1, StringComparer.OrdinalIgnoreCase)
			.ToDictionary(g => g.Key, g => g.First().Item2, StringComparer.OrdinalIgnoreCase);

		public readonly TypeDyn Childs;

		public Class(string name, Class? extends, Class[] implements, bool included = false)
		{
			Name = name;
			Extends = extends;
			Implements = implements;
			Included = included;

			if (this is not TypeDyn)
				Childs = new(this);
			else
				Childs = null!;
			extends?.Childs.Child.Add(this);
		}

		public static Class Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromC is not null || fromF is not null || from.Any())
				throw new Exception();

			string[] m = captures[7].Value.Trim().Split(", ");
			var c = new Class(
				captures[2],
				string.IsNullOrWhiteSpace(m[0]) ? null : Program.GetClass(m[0]),
				m.Skip(1).Select(Program.GetInterface).ToArray());
			Program.Tokens.Add(c.Name, c);

			foreach (var t in lines.Extract().Parse(c, null, Array.Empty<Token>()))
			{
				switch (t)
				{
					case Func func:
						break;
					case Declaration d:
						c.Variables.Add(d);
						break;
					default:
						throw new Exception();
				}
			}

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

			yield return $"typedef struct {Name} {{";
			foreach (var v in AllVariables)
				yield return $"\t{v.ReturnType} {v.Name}";
			yield return $"}}{Name}";
		}

		public static Func ConstructorDeclaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromC is null)
				throw new Exception();

			string name = captures[1];
			string _params = captures[3];

			var fr = lines.Extract();
			int id = fromC.Constructors.Count / 2;

			//Base
			Type type = new Type(fromC, false, true, false, true, false);
			var f = new Func(
				new Type(Program.VOID, false, false, false, false, false),
				(string.IsNullOrWhiteSpace(_params) ? Array.Empty<(Type Type, string Name)>()
					: _params.Split(", ").Select(p =>
					{
						string[] p2 = p.Split(" ");
						return (Program.GetType(string.Join(' ', p2.SkipLast(1)), fromC), p2.Last());
					})).Prepend((type, "this")).ToArray())
			{
				Name = $"{fromC.Name}_Base_{name}_{id}"
			};
			fromC.Constructors.Add(f);

			foreach (var t in fr.Parse(fromC, f, from))
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
				string.IsNullOrWhiteSpace(_params) ? Array.Empty<(Type Type, string Name)>()
					: _params.Split(", ").Select(p =>
					{
						string[] p2 = p.Split(" ");
						return (Program.GetType(string.Join(' ', p2.SkipLast(1)), fromC), p2.Last());
					}).ToArray())
			{
				Name = $"{fromC.Name}_{name}_{id}"
			};
			f.Objects.Add("this", new(type, "this"));

			fromC.Constructors.Add(f);

			f.Actions.Add(new Declaration(type, null) { Name = "this" });

			foreach (var t in fr.Parse(fromC, f, from))
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
