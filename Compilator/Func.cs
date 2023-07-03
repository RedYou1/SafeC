using PCRE;

namespace RedRust
{
	public class Func : Token
	{
		private bool Included = false;
		private List<Token> ToInclude = new();

		public required string Name { get; init; }
		public readonly Type? ReturnType;
		public readonly (Type Type, string Name)[] Params;
		public readonly List<Action> Actions = new();

		public readonly Dictionary<string, Object> Objects = new();

		public Func(Type? returnType, (Type Type, string Name)[] _params)
		{
			ReturnType = returnType;
			Params = _params;

			if (returnType is not null)
				ToInclude.Add(returnType.Of);
			ToInclude.AddRange(Params.Select(p => p.Type.Of));

			foreach (var param in _params)
			{
				Objects.Add(param.Name, new(param.Type));
			}
		}

		public static Func Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is not null || from.Any())
				throw new Exception();

			string returnTypeString = captures[1];
			Type returnType = Program.GetType(returnTypeString, fromC);

			string _params = captures[13];

			var f = new Func(
				returnType,
				string.IsNullOrWhiteSpace(_params) ? Array.Empty<(Type Type, string Name)>()
					: _params.Split(", ").Select(p =>
					{
						string[] p2 = p.Split(" ");
						if (p2.Last().Contains("this"))
							p2 = p2.Append("this").ToArray();
						return (Program.GetType(string.Join(' ', p2.SkipLast(1)), fromC), p2.Last());
					}).ToArray())
			{
				Name = captures[11]
			};

			if (fromC is null)
				Program.Tokens.Add(f.Name, f);
			else
				fromC.Funcs.Add(f.Name, f);


			foreach (var t in lines.Extract().Parse(fromC, f, Array.Empty<Token>()))
			{
				if (t is not Action a)
					throw new Exception();
				f.Actions.Add(a);
			}

			return f;
		}

		public void Compile(StreamWriter output)
		{
			if (Included)
				return;
			Included = true;

			foreach (Token t in ToInclude)
				t.Compile(output);

			output.Write($"{(ReturnType is null ? "void" : ReturnType)} {Name}({string.Join(", ", Params.Select(p => $"{p.Type} {p.Name}"))}){{\n");
			foreach (Action a in Actions)
			{
				a.Compile(output);
			}
			output.Write("\n}\n");
		}
	}
}
