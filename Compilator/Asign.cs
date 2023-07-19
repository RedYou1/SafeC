using PCRE;
using System;

namespace RedRust
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

		public static Asign Declaration(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromF is null)
				throw new Exception();

			Object ob = new FileReader(captures[1]).Parse(fromC, fromF, from).Cast<Object>().First();

			if (!ob.Own)
				throw new Exception();

			Action a = new FileReader(captures[5]).Parse(fromC, fromF, from).Cast<Action>().First();
			IEnumerable<Action>? c = ob.ReturnType.Convert(a, fromF);

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
