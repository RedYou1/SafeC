using PCRE;

namespace RedRust
{
	public class Class : Token
	{
		private bool Included;
		private List<Token> ToInclude;

		public required string Name { get; init; }
		public Class? Extends;
		public Class[] Implements;

		public readonly List<Declaration> Variables = new();
		public readonly Dictionary<string, Func> Funcs = new();
		public readonly List<Func> Constructors = new();

		public IEnumerable<Declaration> AllVariables
		{
			get
			{
				foreach (var v in Variables)
					yield return v;
				if (Extends is not null)
					foreach (var v in Extends.AllVariables)
						yield return v;
			}
		}

		public IEnumerable<Func> AllFuncs
		{
			get
			{
				foreach (var f in Funcs)
					yield return f.Value;
				if (Extends is not null)
					foreach (var f in Extends.AllFuncs)
						yield return f;
			}
		}

		public Class(Class? extends, Class[] implements, bool included = false)
		{
			Extends = extends;
			Implements = implements;
			Included = included;

			ToInclude = new();
			if (extends is not null)
				ToInclude.Add(extends);
			ToInclude.AddRange(implements);
		}


		public static Class Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromC is not null || fromF is not null || from.Any())
				throw new Exception();

			string[] m = captures.Groups[7].Value.Trim().Split(", ");
			var c = new Class(
				string.IsNullOrWhiteSpace(m[0]) ? null : Program.GetClass(m[0]),
				m.Skip(1).Select(Program.GetInterface).ToArray())
			{ Name = captures.Groups[2] };
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

		public void Compile(StreamWriter output)
		{
			if (Included)
				return;
			Included = true;

			foreach (Token t in ToInclude)
				t.Compile(output);

			output.Write($"typedef struct {Name}{{\n\n}}{Name};\n");
		}

		public static Func ConstructorDeclaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromC is null)
				throw new Exception();

			string name = captures[1];
			Type returnType = Program.GetType(name, fromC);

			string _params = captures[3];

			var f = new Func(
				returnType,
				string.IsNullOrWhiteSpace(_params) ? Array.Empty<(Type Type, string Name)>()
					: _params.Split(", ").Select(p =>
					{
						string[] p2 = p.Split(" ");
						return (Program.GetType(string.Join(' ', p2.SkipLast(1)), fromC), p2.Last());
					}).ToArray())
			{
				Name = name
			};

			fromC.Constructors.Add(f);

			foreach (var t in lines.Extract().Parse(fromC, f, from))
			{
				if (t is not Action a)
					throw new Exception();
				f.Actions.Add(a);
			}

			return f;
		}
	}
}
