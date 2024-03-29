﻿namespace SafeC
{
	[Class("u64", "unsigned long", null, new string[] { "INumber" })]
	internal class ULong
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%lu\", {ob}";
		}
	}
}
