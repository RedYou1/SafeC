using PCRE;

namespace RedRust
{
	public class CallFunction : Action
	{
		public Type ReturnType => Func.ReturnType ?? throw new Exception();

		public string Name => throw new NotImplementedException();

		public readonly Func Func;
		public readonly Action[] Args;

		public CallFunction(Func func, Action[] args)
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
				f = ob.ReturnType.Of.AllFuncs.First(f => f.Name.Equals(name));
			}

			if (f is null)
				throw new Exception();

			string[] argsStr = captures[4].Value.Split(", ");
			if (string.IsNullOrWhiteSpace(argsStr[0]))
				argsStr = Array.Empty<string>();

			Action[] args = new FileReader(argsStr).Parse(fromC, fromF, from).Cast<Action>().ToArray();
			if (ob is not null)
				args = args.Prepend(ob).ToArray();
			//TODO

			if (f.Params.Length != args.Length)
				throw new Exception();

			return new CallFunction(f, args);
		}

		public static CallFunction BaseDeclaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromC is null || fromC.Extends is null || fromF is null)
				throw new Exception();

			string[] argsStr = captures[1].Value.Split(", ");
			if (string.IsNullOrWhiteSpace(argsStr[0]))
				argsStr = Array.Empty<string>();
			Action[] args = new FileReader(argsStr).Parse(fromC, fromF, from).Cast<Action>().ToArray();

			var func = fromC.Extends.Constructors.Where(c => c.Params.Length == args.Length).ToArray();

			if (func.Length != 1)
				throw new Exception();

			return new CallFunction(func[0], args);
		}

		public static CallFunction NewDeclaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is null)
				throw new Exception();

			string[] argsStr = captures[6].Value.Split(", ");
			if (string.IsNullOrWhiteSpace(argsStr[0]))
				argsStr = Array.Empty<string>();
			Action[] args = new FileReader(argsStr).Parse(fromC, fromF, from).Cast<Action>().ToArray();

			var func = Program.GetClass(captures[1]).Constructors.Where(c => c.Params.Length == args.Length).ToArray();

			if (func.Length != 1)
				throw new Exception();

			return new CallFunction(func[0], args);
		}

		public void Compile(StreamWriter output)
		{
			throw new NotImplementedException();
		}
	}
}
