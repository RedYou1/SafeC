
namespace RedRust
{
	[Class("str", "char*", null, new string[0])]
	public class Str
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Func(
			"Len", null,
			"u64",
			"str", "this")]
		public static IEnumerable<string> Len()
		{
			yield return $"return StrLen(this)";
		}

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
