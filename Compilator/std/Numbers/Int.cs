﻿namespace SafeC
{
	[Class("i32", "int", null, new string[] { "INumber" })]
	internal class Int
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%i\", {ob}";
		}
	}
}
