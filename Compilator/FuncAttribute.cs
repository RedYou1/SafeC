namespace RedRust
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	internal class FuncAttribute : Attribute
	{
		public readonly string Name;
		public readonly string? CName;
		public readonly string Return;
		public readonly string[] Params;

		public FuncAttribute(string name, string? cName, string @return, params string[] @params)
		{
			Name = name;
			CName = cName;

			if (@params.Length % 2 == 1)
				throw new Exception();

			Return = @return;
			Params = @params;
		}

		public (Type @return, Parameter[] @params) ApplyFunc()
		{
			var @return = Program.GetType(Return, null);
			var @params = Params.Chunk(2).Select((t, i) => new Parameter(Program.GetType(t[0], null), t[1])).ToArray();

			return (@return, @params);
		}
	}
}
