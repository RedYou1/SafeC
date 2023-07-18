using PCRE;
using System.Text;

namespace RedRust
{
	public class Func : Token
	{
		public bool Included { get; set; }

		public required string Name { get; init; }
		public readonly Type? ReturnType;
		public readonly (Type Type, string Name)[] Params;
		public readonly List<Action> Actions = new();

		public readonly Dictionary<string, Object> Objects = new();

		public Func(Type? returnType, (Type Type, string Name)[] _params)
		{
			ReturnType = returnType;
			Params = _params;

			foreach (var param in Params)
			{
				Objects.Add(param.Name, new(param.Type, param.Name));
			}
		}

		public static Func Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is not null || from.Any())
				throw new Exception();

			string returnTypeString = captures[1];
			Type returnType = Program.GetType(returnTypeString, fromC);

			string _params = captures[13];

			string fRawName = captures[11];
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
				Name = $"{(fromC is null ? "" : $"{fromC.Name}_")}{fRawName}"
			};

			if (fromC is null)
				Program.Tokens.Add(fRawName, f);
			else
				fromC.Funcs.Add(fRawName, f);


			foreach (var t in lines.Extract().Parse(fromC, f, Array.Empty<Token>()))
			{
				if (t is not Action a)
					throw new Exception();
				f.Actions.Add(a);
			}

			return f;
		}

		public IEnumerable<Action>[]? CanCall(Func from, params Action[] args)
		{
			if (Params.Length != args.Length)
				return null;

			IEnumerable<Action>[] converts = new IEnumerable<Action>[Params.Length];

			for (int i = 0; i < Params.Length; i++)
			{
				converts[i] = Params[i].Type.Convert(args[i], from)!;
				if (converts[i] is null)
					return null;
			}
			return converts;
		}

		public IEnumerable<Token> ToInclude()
		{
			if (Included)
				yield break;

			if (ReturnType is not null)
				foreach (var t in ReturnType.ToInclude())
					yield return t;
			foreach (var p in Params)
				foreach (var t in p.Type.ToInclude())
					yield return t;
			foreach (var a in Actions)
				foreach (var t in a.ToInclude())
					yield return t;
		}

		public virtual IEnumerable<string> Compile()
		{
			if (Included)
				yield break;
			Included = true;

			yield return $"{(ReturnType is null ? "void" : ReturnType)} {Name}({string.Join(", ", Params.Select(p => $"{p.Type} {p.Name}"))}) {{";
			foreach (Action a in Actions)
				foreach (string s in a.Compile())
					yield return $"\t{s}";
			yield return "}";
		}
	}
}
