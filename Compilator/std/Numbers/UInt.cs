﻿namespace RedRust
{
	[Class("u32", "unsigned int", null, new string[] { "INumber" })]
	public class UInt
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%u\", {ob}";
		}
	}
}
