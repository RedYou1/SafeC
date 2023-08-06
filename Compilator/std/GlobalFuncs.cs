namespace SafeC
{
	internal class GlobalFuncs
	{
		[Func(
			"print", "printf",
			"void",
			"str", "s")]
		public static IEnumerable<StdLine> print() { return Enumerable.Empty<StdLine>(); }

		[Func(
			"StrLen", "strlen",
			"u64",
			"str", "this")]
		public static IEnumerable<StdLine> StrLen() { return Enumerable.Empty<StdLine>(); }
	}
}
