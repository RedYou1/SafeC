namespace RedRust
{
	internal class Declaration : ActionLine
	{
		public readonly string Name;
		public readonly Type Type;
		public Type ReturnType => Type;
		public readonly ActionLine? Value;

		public Declaration(Func of, string name, Type type, ActionLine? value)
		{
			Name = name;
			Type = type;
			Value = value;
			if (Value is not null && of.ReturnType != type)//dyntype
				throw new NotImplementedException();
			DoAction(of.Memory);
		}

		public Declaration(string name, Type type, ActionLine? value)
		{
			Name = name;
			Type = type;
			Value = value;
		}

		public (string, Object?) DoAction(Memory mem)
		{
			(string, Object?) t = (string.Empty, null);
			if (Value is not null)
				t = Value.DoAction(mem);
			mem.AddVar(Name, t.Item2);
			return new($"{Type.Name} {Name}{(Value is null ? "" : $" = {t}")};", t.Item2);
		}
	}
}
