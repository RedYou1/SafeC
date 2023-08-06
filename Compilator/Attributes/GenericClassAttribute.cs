namespace SafeC
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	internal class GenericClassAttribute : GenericAttribute<Class>, IClassAttribute
	{
		public string Name { get; }
		public string? Extends { get; }
		public string[] Implements { get; }

		public GenericClassAttribute(string name, string? extends, string[] implements, string[] generics)
			: base(generics)
		{
			Name = name;
			Extends = extends;
			Implements = implements;
		}
	}
}
