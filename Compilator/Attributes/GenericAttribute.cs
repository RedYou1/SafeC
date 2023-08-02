namespace RedRust
{
	internal abstract class GenericAttribute<T> : Attribute
	{
		public readonly string[] Generics;

		public GenericAttribute(string[] generics)
		{
			Generics = generics;
		}
	}
}
