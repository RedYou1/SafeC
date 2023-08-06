using PCRE;

namespace SafeC
{
	public class Asign : Action
	{
		public required string Name { get; init; }

		public Type ReturnType => throw new NotImplementedException();

		public readonly IEnumerable<Action> Actions;

		public Asign(IEnumerable<Action> actions)
		{
			Actions = actions;
		}

		public static Asign Declaration(FileReader lines, PcreMatch captures, IClass? fromC, Func? fromF, Dictionary<string, Class>? gen, Token[] from)
		{
			if (fromF is null)
				throw NotInRigthPlacesException.Func("Asign");

			Object ob = new FileReader(captures[1].Value).Parse(fromC, fromF, gen, from).Cast<Object>().First();

			if (!ob.Own)
				throw new NoAccessException(ob.Name);

			Action a = new FileReader(captures[9].Value).Parse(fromC, fromF, gen, from).Cast<Action>().First();
			IEnumerable<Action>? c = ob.ReturnType.Convert(a, fromF, gen);

			if (c is null)
				throw new Exception();

			return new Asign(c)
			{ Name = ob.Name };
		}

		public IEnumerable<Token> ToInclude()
		{
			foreach (Action action in Actions)
				foreach (Token t in action.ToInclude())
					yield return t;
		}

		public IEnumerable<string> Compile()
		{
			foreach (Action a in Actions.SkipLast(1))
				foreach (string sss in a.Compile())
					yield return sss;

			var s = Actions.Last().Compile();
			foreach (string ss in s.SkipLast(1))
				yield return ss;
			yield return $"{Name} = {s.Last()}";
		}
	}
}
