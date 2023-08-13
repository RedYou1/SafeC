using PCRE;

namespace SafeC
{
	internal class Func : IFunc
	{
		public bool Included { get; set; }

		public required string Name { get; init; }
		public readonly Type ReturnType;
		public readonly Parameter[] Params;
		public readonly List<ActionContainer> Actions = new();

		public readonly Dictionary<string, Object> Objects = new();

		public Func(Type returnType, Parameter[] _params, bool included = false)
		{
			Included = included;
			ReturnType = returnType;
			Params = _params;

			foreach (var param in Params)
			{
				Objects.Add(param.Name, new(param.Type, param.Name));
			}
		}

		public static IFunc Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromF is not null || from.Any())
				throw NotInRigthPlacesException.NoFunc("Func");

			Class? fc = fromC as Class;
			if (fc is null != fromC is null)
				throw new Exception();

			string returnTypeString = captures[1];
			Type returnType = Compiler.Instance!.GetType(returnTypeString, fromC, gen, false);

			string[] _params = captures[13].Value.Split(", ");

			string fRawName = captures[11];

			string name;
			string[] gens = Array.Empty<string>();
			int tname = fRawName.IndexOf('<');
			if (tname < 0)
				name = fRawName;
			else
			{
				name = fRawName.Substring(0, tname);
				gens = fRawName.Substring(tname + 1, fRawName.Length - tname - 2).Split(", ");
			}

			name = $"{(fc is null ? "" : $"{fc.TypeName}_")}{name}";

			var fr = lines.Extract();

			IFunc f;
			if (gens.Length == 0)
			{
				f = new Func(
					returnType,
					string.IsNullOrWhiteSpace(_params[0]) ? Array.Empty<Parameter>()
						: _params.Select(p =>
						{
							string[] p2 = p.Split(" ");
							if (p2.Last().Contains("this"))
								p2 = p2.Append("this").ToArray();
							return new Parameter(Compiler.Instance!.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen, false), p2.Last());
						}).ToArray())
				{
					Name = name
				};
			}
			else
			{
				f = new GenericFunc(fromC, name, string.IsNullOrWhiteSpace(_params[0]) ? 0 : _params.Length,
					gen, gens,
					_ =>
					{
						fr.Line = 0;
						return fr;
					},
					_ => returnType,
					gen2 => _params.Select(p =>
						{
							string[] p2 = p.Split(" ");
							if (p2.Last().Contains("this"))
								p2 = p2.Append("this").ToArray();
							return new Parameter(Compiler.Instance!.GetType(string.Join(' ', p2.SkipLast(1)), fromC, gen2, false), p2.Last());
						}).ToArray());
			}

			if (fromC is Class c)
				c.Funcs.Add(fRawName, f);
			else
				Compiler.Instance!.Tokens.Add(fRawName, f);


			if (f is Func rf)
			{
				foreach (var t in fr.Parse(fromC, rf, gen, Array.Empty<Token>()))
				{
					if (t is not ActionContainer a)
						throw new Exception();
					rf.Actions.Add(a);
				}
			}

			return f;
		}

		public IFunc.CanCallReturn? CanCall(Func from, Dictionary<string, Class>? gen, params Action[] args)
		{
			if (Params.Length != args.Length)
				return null;

			IEnumerable<ActionContainer>[] converts = new IEnumerable<ActionContainer>[Params.Length];

			for (int i = 0; i < Params.Length; i++)
			{
				converts[i] = Params[i].Type.Convert(args[i], from, gen)!;
				if (converts[i] is null)
					return null;
			}
			return new(this, converts);
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

		public IEnumerable<string> Compile()
		{
			if (Included)
				yield break;
			Included = true;

			yield return $"{(ReturnType is null ? "void" : ReturnType)} {Name}({string.Join(", ", Params.Select(p => $"{p.Type} {p.Name}"))}) {{";
			foreach (var a in Actions)
				foreach (string s in a.Compile())
					yield return $"\t{s}";
			yield return "}";
		}
	}
}
