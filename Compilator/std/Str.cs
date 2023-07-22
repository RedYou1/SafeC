
namespace RedRust
{
	[Class("str", "char*")]
	public class Str
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Func(
			"StartsWith", null,
			"bool",
			"str", "this",
			"str", "s")]
		public static IEnumerable<string> StartsWith()
		{
			yield break;
		}
	}
}
