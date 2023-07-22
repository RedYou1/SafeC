namespace RedRust
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	internal class ClassAttribute : Attribute
	{
		public readonly string Name;
		public readonly string? CName;

		public ClassAttribute(string name, string? cName)
		{
			Name = name;
			CName = cName;
		}
	}
}
