namespace SafeC
{
	internal class GlobalFuncs
	{
		[Func(
			"print", "printf",
			"void",
			"str", "s")]
		public static IEnumerable<StdLine> print() { return Enumerable.Empty<StdLine>(); }
	}
}
