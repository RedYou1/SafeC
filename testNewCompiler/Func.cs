namespace RedRust
{
	internal class Func : Definition
	{
		private static readonly List<string> Names = new();

		public static string CheckName(string name)
		{
			int i = 1;
			string temp = name;

			while (true)
			{
				if (!Names.Contains(temp))
				{
					Names.Add(temp);
					return temp;
				}
				i++;
				temp = $"{name}{i}";
			}
		}


		private bool compiled = false;
		public readonly Class? Of;

		public readonly IEnumerable<KeyValuePair<string, Type>> Params;
		public readonly Action Action;
		public Type? ReturnType => Action.ReturnType;

		public Func(string name, Class? of, IEnumerable<KeyValuePair<string, Type>> _params, Action action)
			: base(CheckName(name), $"{FuncSep}{(of is null ? "" : $"{of.FullName}{FuncSep}")}{name}")
		{
			Of = of;

			AddDef(FullName, this);
			Params = _params;
			Action = action;
		}

		public override void Compile()
		{
			if (compiled)
				return;
			compiled = true;

			StreamWriter sw = Compiler.Instance.StreamWriter;

			sw.Write($"{(ReturnType is null ? "void" : ReturnType.Name)} {Name}({string.Join(',', Params.Select(l => $"{l.Value.Name} {l.Key}"))})");
			sw.WriteLine(Action.DoAction(new Memory(null, Params.ToDictionary(l => l.Key, l => new Object(l.Value)))));
		}
	}
}
