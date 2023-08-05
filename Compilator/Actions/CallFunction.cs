using PCRE;
using System.Text;

namespace RedRust
{
	public class CallFunction : Action
	{
		public Type ReturnType => Func.ReturnType ?? throw new Exception();

		public string Name => throw new NotImplementedException();

		public readonly Func Func;
		public readonly IEnumerable<Action>[] Args;

		public CallFunction(Func func, IEnumerable<Action>[] args)
		{
			Func = func;
			Args = args;
		}

		private static IEnumerable<string> getArgs(string args)
		{
			string t;
			while (args.Any())
			{
				switch (args[0])
				{
					case '"':
						t = string.Join(null, args.Skip(1).TakeWhile(x => x != '"'));
						yield return $"\"{t}\"";
						if (args.Length <= t.Length + 4)
							yield break;
						args = args.Substring(t.Length + 4);
						break;
					default:
						t = string.Join(null, args.TakeWhile(x => x != ','));
						yield return t;
						if (args.Length <= t.Length + 2)
							yield break;
						args = args.Substring(t.Length + 2);
						break;
				}
			}
		}

		public static CallFunction Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromF is null)
				throw NotInRigthPlacesException.Func("CallFunction");

			string name = captures[1].Value.Split('.').Last();
			string[] of = captures[1].Value.Split('.').SkipLast(1).ToArray();

			Object? ob = null;
			IFunc? f;
			if (of.Length == 0)
			{
				f = Compiler.Instance!.Tokens[name] as IFunc;

				if (f is null)
					throw new CompileException($"function {name} not found");
			}
			else
			{
				ob = new FileReader(string.Join('.', of)).Parse(fromC, fromF, gen, from).Cast<Object>().First();

				if (!ob.Own)
					throw new NoAccessException(ob.Name);

				f = ob.ReturnType.Of.AllFuncs[name];

				if (f is null)
					throw new CompileException($"function {name} not found of class {ob.Of.Name}");
			}


			Action[] args = new FileReader(getArgs(captures[9]).ToStdLine().ToArray()).Parse(fromC, fromF, gen, from)
				.Cast<Action>().ToArray();
			if (ob is not null)
				args = args.Prepend(ob).ToArray();
			//TODO

			var c = f.CanCall(fromF, gen, args);

			if (c is null)
				throw new CompileException($"cant call function {f.Name}");

			return new CallFunction(c.Func, c.Args);
		}

		public static CallFunction BaseDeclaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromC is null || fromC.Extends is null || fromF is null)
				throw NotInRigthPlacesException.ClassOrFunc("CallFunction base");

			Action[] args = new FileReader(getArgs(captures[1]).ToStdLine().ToArray())
				.Parse(fromC, fromF, gen, from).Cast<Action>().Prepend(fromF.Objects["this"]).ToArray();

			var func = fromC.Extends.Constructors.Select(c => c.CanCall(fromF, gen, args)).Where(c => c is not null).ToArray();

			if (func.Length != 1)
				throw new CompileException($"function found {func.Length} in CallFunc base");

			return new CallFunction(func[0]!.Func, func[0]!.Args);
		}

		public static CallFunction NewDeclaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromF is null)
				throw NotInRigthPlacesException.Func("CallFunction new");

			Class @class = IClass.IsClass(Compiler.Instance!.GetClass(captures[4], gen));

			//string gensClass = captures[7];

			string tgensConstructor = captures[2];

			string[] gensConstructor = string.IsNullOrEmpty(tgensConstructor) ?
				Array.Empty<string>() : tgensConstructor.Split(", ");

			Dictionary<string, Class>? gens = gen is null ? null : new(gen);

			var func = @class.Constructors.Select(c =>
			{
				Dictionary<string, Class>? gen3 = gens is null ? null : new(gens);
				if (c is GenericFunc gf)
				{
					if (gensConstructor.Length != gf.GenNames.Length)
						throw new Exception();
					if (gen3 is null)
						gen3 = new();
					for (int i = 0; i < gf.GenNames.Length; i++)
						gen3.Add(gf.GenNames[i], IClass.IsClass(Compiler.Instance!.GetClass(gensConstructor[i], gen)));
				}
				else if (c is not RedRust.Func)
					throw new Exception();

				Action[] args = new FileReader(getArgs(captures[9]).ToStdLine().ToArray()).Parse(fromC, fromF, gen3, from).Cast<Action>().ToArray();

				return c.CanCall(fromF, gen3, args);
			}).Where(c => c is not null).ToArray();

			if (func.Length != 1)
				throw new CompileException($"function found {func.Length} in CallFunc new");

			return new CallFunction(func[0]!.Func, func[0]!.Args);
		}

		public IEnumerable<Token> ToInclude()
		{
			yield return Func;

			for (int i = 0; i < Args.Length; i++)
			{
				foreach (Action p in Args[i])
					foreach (var a in p.ToInclude())
						yield return a;
			}
		}

		public IEnumerable<string> Compile()
		{
			StringBuilder s = new($"{Func.Name}(");
			bool not_first = false;
			for (int i = 0; i < Args.Length; i++)
			{
				IEnumerable<Action> pp = Args[i];

				foreach (Action ppp in pp.SkipLast(1))
					foreach (string sss in ppp.Compile())
						yield return sss;

				var p = pp.Last();
				var ss = p.Compile();
				foreach (string sss in ss.SkipLast(1))
					yield return sss;
				if (not_first)
					s.Append(", ");
				s.Append(ss.Last());
				not_first = true;
			}
			s.Append(")");
			yield return s.ToString();
		}
	}
}
