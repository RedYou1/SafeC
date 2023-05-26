namespace RedRust
{
	internal class Function : Incluer
	{
		public List<Includable> includes { get; } = new();
		public bool included { get; set; } = false;


		public readonly Type returnType;
		public readonly string name;
		public readonly Variable[] parameters;
		public readonly Token[] lines;

		public Function(string name, Type returnType, Variable[] parameters, List<Includable> includes, params Token[] lines)
		{
			this.name = name;
			this.returnType = returnType;
			this.parameters = parameters;
			this.lines = lines;

			this.includes = includes;
			this.includes.AddRange(parameters.Select(v => v.type is Includable i ? i : null)
				.Where(v => v is not null).Cast<Includable>());
			if (returnType is Includable i)
				this.includes.Add(i);
		}

		public List<Converter>[]? CanExecute((Type type, LifeTime lifeTime)[] vars)
		{
			if (parameters.Length != vars.Length)
				return null;
			List<Converter>[] functions = new List<Converter>[vars.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				var function = parameters[i].type.Equivalent(vars[i].type);
				if (function is null)
					return null;
				functions[i] = function;
			}
			return functions;
		}

		public void Compile(string tabs, StreamWriter sw)
		{
			if (included)
				return;
			included = true;

			foreach (var include in includes)
				include.Compile(tabs, sw);

			sw.Write($"{tabs}{returnType.id} {name}(");
			if (parameters.Any())
			{
				sw.Write($"{parameters.First().type.id} {parameters.First().name}");
				foreach (var parameter in parameters.Skip(1))
				{
					sw.Write($", {parameter.type.id} {parameter.name}");
				}
			}
			sw.WriteLine(") {");
			foreach (var line in lines)
			{
				line.Compile($"{tabs}\t", sw);
			}
			sw.WriteLine($$"""{{tabs}}}""");
		}
	}

	internal class FuncBlock : Token
	{
		public readonly Token[] lines;

		public FuncBlock(params Token[] lines)
		{
			this.lines = lines;
		}

		public void Compile(string tabs, StreamWriter sw)
		{
			foreach (var line in lines)
			{
				line.Compile($"{tabs}\t", sw);
			}
		}
	}
}
