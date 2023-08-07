
namespace SafeC
{
	[Class("str", "char*", null, new string[0])]
	internal class Str
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Func(
			"Len", null,
			"u64",
			"str", "this")]
		public static IEnumerable<StdLine> Len()
		{
			yield return $"return StrLen(this)";
		}

		[Func(
			"StartsWith", null,
			"bool",
			"str", "this",
			"str", "s")]
		public static IEnumerable<StdLine> StartsWith()
		{
			yield break;
		}
	}
}
