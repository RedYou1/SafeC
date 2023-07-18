using System.Diagnostics.CodeAnalysis;

namespace RedRust
{
	public class IntToString : Action
	{
		public Type ReturnType => new(Program.STR, true, false, false, false, false);

		public string Name => throw new NotImplementedException();

		public readonly Action Action;

		public IntToString(Action action)
		{
			Action = action;
		}

		public IEnumerable<string> Compile()
		{
			var a = Action.Compile();
			foreach (var c in a.SkipLast(1))
				yield return c;
			yield return $"\"%i\", {a.Last()}";
		}

		public IEnumerable<Token> ToInclude()
		{
			yield return Program.STR;
			foreach (var c in Action.ToInclude())
				yield return c;
		}
	}

	public class Int : Class
	{
		public Int() : base("int", null, Array.Empty<Class>(), true)
		{
		}

		public void Init()
		{
			Casts.Add(Program.STR, (a) => new IntToString(a));
		}
	}
}
