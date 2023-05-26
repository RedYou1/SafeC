namespace RedRust
{
	internal interface ActionLine
	{
		public Type? ReturnType { get; }
		public (string, Object?) DoAction(Memory memory);
	}

	internal class Action : ActionLine
	{
		private ActionLine[] Lines;
		public Type? ReturnType => null;

		public Action(params ActionLine[] lines)
		{
			if (lines.Length == 0)
				throw new Exception("not enough lines");
			Lines = lines;
		}

		public (string, Object?) DoAction(Memory mem)
		{
			StreamWriter sw = Compiler.Instance.StreamWriter;

			if (Lines.Length == 1)
			{
				return new($"\t{Lines[0].DoAction(mem)}", null);
			}

			return new(
				$"{{{Environment.NewLine}{Lines.Select(l => $"\t{l.DoAction(mem)};{Environment.NewLine}")}{Environment.NewLine}}}",
				null);
		}
	}
}
