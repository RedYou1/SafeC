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

		public static CallFunction Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is null)
				throw new Exception();

			string name = captures[1].Value.Split('.').Last();
			string[] of = captures[1].Value.Split('.').SkipLast(1).ToArray();

			Object? ob = null;
			Func? f;
			if (of.Length == 0)
				f = Program.Tokens[name] as Func;
			else
			{
				ob = new FileReader(string.Join('.', of)).Parse(fromC, fromF, from).Cast<Object>().First();
				f = ob.ReturnType.Of.AllFuncs[name];
			}

			if (f is null)
				throw new Exception();

			string[] argsStr = captures[5].Value.Split(", ");
			if (string.IsNullOrWhiteSpace(argsStr[0]))
				argsStr = Array.Empty<string>();

			Action[] args = new FileReader(argsStr).Parse(fromC, fromF, from).Cast<Action>().ToArray();
			if (ob is not null)
				args = args.Prepend(ob).ToArray();
			//TODO

			var c = f.CanCall(fromF, args);

			if (c is null)
				throw new Exception();

			return new CallFunction(f, c);
		}

		public static CallFunction BaseDeclaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromC is null || fromC.Extends is null || fromF is null)
				throw new Exception();

			string[] argsStr = captures[1].Value.Split(", ");
			if (string.IsNullOrWhiteSpace(argsStr[0]))
				argsStr = Array.Empty<string>();
			Action[] args = new FileReader(argsStr).Parse(fromC, fromF, from).Cast<Action>().Prepend(fromF.Objects["this"]).ToArray();

			var func = fromC.Extends.Constructors.Select(c => (c, c.CanCall(fromF, args))).Where(c => c.Item2 is not null).ToArray();

			if (func.Length != 1)
				throw new Exception();

			return new CallFunction(func[0].Item1, func[0].Item2!);
		}

		public static CallFunction NewDeclaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is null)
				throw new Exception();

			string[] argsStr = captures[6].Value.Split(", ");
			if (string.IsNullOrWhiteSpace(argsStr[0]))
				argsStr = Array.Empty<string>();
			Action[] args = new FileReader(argsStr).Parse(fromC, fromF, from).Cast<Action>().ToArray();

			var func = Program.GetClass(captures[1]).Constructors.Select(c => (c, c.CanCall(fromF, args))).Where(c => c.Item2 is not null).ToArray();

			if (func.Length != 1)
				throw new Exception();

			return new CallFunction(func[0].Item1, func[0].Item2!);
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
