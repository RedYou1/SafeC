namespace RedRust
{
	public class StdLine
	{
		public readonly bool Unsafe;
		public readonly string Line;
		public readonly Action<Func>? Action;

		private StdLine(bool @unsafe, string line)
		{
			Unsafe = @unsafe;
			Line = line;
			Action = null;
		}

		public StdLine(string line, Action<Func> action)
		{
			Unsafe = true;
			Line = line;
			Action = action;
		}

		public static implicit operator StdLine(string s) => new(false, s);
	}
}
