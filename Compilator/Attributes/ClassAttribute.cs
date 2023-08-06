namespace SafeC
{
	internal interface IClassAttribute
	{
		string Name { get; }
		string? Extends { get; }
		string[] Implements { get; }
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	internal class ClassAttribute : Attribute, IClassAttribute
	{
		public string Name { get; }
		public string? CName;
		public string? Extends { get; }
		public string[] Implements { get; }

		public ClassAttribute(string name, string? cName, string? extends, string[] implements)
		{
			Name = name;
			CName = cName;
			Extends = extends;
			Implements = implements;
		}
	}
}
