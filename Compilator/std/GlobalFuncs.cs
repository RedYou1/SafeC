namespace RedRust
{
	internal class GlobalFuncs
	{
		[Func(
			"print", "printf",
			"void",
			"str", "s")]
		public static IEnumerable<string> print() { return Enumerable.Empty<string>(); }
	}
}
