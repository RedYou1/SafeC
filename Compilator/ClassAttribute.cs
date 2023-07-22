namespace RedRust
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	internal class ClassAttribute : Attribute
	{
		public readonly string Name;
		public readonly string? CName;
		public readonly string? Extends;
		public readonly string[] Implements;

		public ClassAttribute(string name, string? cName, string? extends, string[] implements)
		{
			Name = name;
			CName = cName;
			Extends = extends;
			Implements = implements;
		}
	}
}
