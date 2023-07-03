using PCRE;

namespace RedRust
{
	public class Declaration : Action
	{
		public Type Of;
		public readonly Action? Action;

		public required string Name { get; init; }

		public Type ReturnType
		{
			get
			{
				if (Action is not null && Action.ReturnType.Of != Program.VOID)
					return Action.ReturnType;
				return Of;
			}
		}

		public Declaration(Type of, Action? action)
		{
			Of = of;
			Action = action;
		}

		public static Declaration Declaration_(FileReader lines, PcreMatch captures, Class? fromC, Func? fromF, Token[] from)
		{
			if (fromC is null && fromF is null)
				throw new Exception();

			var r = Program.GetType(captures[1], fromC);

			Action? current = null;
			if (!string.IsNullOrEmpty(captures[13]))
			{
				current = new FileReader(captures[13].Value).Parse(fromC, fromF, from).First() as Action;
				if (current is null)
				{
					throw new Exception();
				}
			}

			var d = new Declaration(r, current) { Name = captures[11] };

			if (fromF is not null)
				fromF.Objects.Add(d.Name, new(d.ReturnType));

			return d;
		}

		public void Compile(StreamWriter output)
		{
			output.Write($"{Of} {Name}");
			if (Action is not null)
			{
				output.Write(" = ");
				Action.Compile(output);
			}
			output.WriteLine(";");
		}
	}
}
