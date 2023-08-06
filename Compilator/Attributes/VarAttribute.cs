namespace SafeC
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	internal class VarAttribute : Attribute
	{
		public readonly string Name;
		public readonly string Type;

		public VarAttribute(string name, string type)
		{
			Name = name;
			Type = type;
		}

		public Type ApplyFunc(Class of)
		{
			return Compiler.Instance!.GetType(Type, of, null);
		}
	}
}
