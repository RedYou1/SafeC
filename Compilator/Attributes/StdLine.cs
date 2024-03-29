﻿namespace SafeC
{
	internal class StdLine
	{
		public readonly bool Unsafe;
		public readonly string Line;
		public readonly Action<IClass?, Func?>? Action;

		private StdLine(bool @unsafe, string line)
		{
			Unsafe = @unsafe;
			Line = line;
			Action = null;
		}

		public StdLine(string line, Action<IClass?, Func?>? action = null)
		{
			Unsafe = true;
			Line = line;
			Action = action;
		}

		public static implicit operator StdLine(string s) => new(false, s);
	}
}
