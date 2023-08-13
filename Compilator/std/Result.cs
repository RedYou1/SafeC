
namespace SafeC
{
	[GenericClass("Result", null, new string[0], new string[] { "T", "E" })]
	internal class Result
	{
		public static IEnumerable<StdLine> Variables(Dictionary<string, Class> gen)
		{
			yield return "bool o";
			yield return "union:";
			yield return "\tT r";
			yield return "\tE e";
		}

		[Func(
			"IsOk", null,
			"bool",
			"Result<T, E>", "this")]
		public static IEnumerable<StdLine> IsOk(Dictionary<string, Class> gen)
		{
			yield return new($"return this.o");
		}

		[Func(
			"IsError", null,
			"bool",
			"Result<T, E>", "this")]
		public static IEnumerable<StdLine> IsError(Dictionary<string, Class> gen)
		{
			yield return new($"return !this.o");
		}

		[Func(
			"Ok", null,
			"T",
			"Result<T, E>", "this")]
		public static IEnumerable<StdLine> Ok(Dictionary<string, Class> gen)
		{
			yield return new($"return this.r");
		}

		[Func(
			"Error", null,
			"E",
			"Result<T, E>", "this")]
		public static IEnumerable<StdLine> Error(Dictionary<string, Class> gen)
		{
			yield return new($"return this.e");
		}
	}
}
