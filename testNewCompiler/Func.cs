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
		public readonly Type? returnType;
		public Action? Action = null;
		public Type? ReturnType => returnType;

		public readonly Memory Memory = new();

		public Func(string name, Class? of, IEnumerable<KeyValuePair<string, Type>> _params, Type? returnType)
			: base(CheckName(name), $"{FuncSep}{(of is null ? "" : $"{of.FullName}{FuncSep}")}{name}")
		{
			Of = of;

			AddDef(FullName, this);
			Params = _params;
			foreach (var p in Params)
				Memory.AddVar(name, new(p.Value));
			this.returnType = returnType;
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
