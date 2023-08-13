namespace SafeC
{
	internal class GlobalFuncs
	{
		[Func(
			"print", "printf",
			"void",
			"str", "s")]
		public static IEnumerable<StdLine> print() { return Enumerable.Empty<StdLine>(); }

		[GenericFunc(
			"Ok", new string[] { "T", "E" },
			"Result<T, E>",
			"T", "r")]
		public static IEnumerable<StdLine> Ok(Dictionary<string, Class> gen)
		{
			yield return $"Result<{gen["T"].Name}, {gen["E"].Name}> this";
			yield return "this.o = true";
			yield return "this.r = r";
			yield return "return this";
		}

		[GenericFunc(
			"Error", new string[] { "T", "E" },
			"Result<T, E>",
			"E", "e")]
		public static IEnumerable<StdLine> Error(Dictionary<string, Class> gen)
		{
			yield return $"Result<{gen["T"].Name}, {gen["E"].Name}> this";
			yield return "this.o = false";
			yield return "this.e = e";
			yield return "return this";
		}
	}
}
