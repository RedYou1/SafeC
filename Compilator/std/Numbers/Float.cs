﻿namespace SafeC
{
	[Class("f32", "float", null, new string[] { "INumber" })]
	internal class Float
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%f\", {ob}";
		}
	}
}
