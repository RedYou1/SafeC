namespace RedRust
{
	internal class GlobalFuncs
	{
		[Func(
			"print", "printf",
			"void",
			"str", "s")]
		public static IEnumerable<string> print() { return Enumerable.Empty<string>(); }

		[Func(
			"StrLen", "strlen",
			"u64",
			"str", "this")]
		public static IEnumerable<string> StrLen() { return Enumerable.Empty<string>(); }
	}
}
