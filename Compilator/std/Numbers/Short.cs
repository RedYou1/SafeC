namespace SafeC
{
	[Class("i16", "short", null, new string[] { "INumber" })]
	internal class Short
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%i\", {ob}";
		}

		[Cast("i32")]
		public static IEnumerable<StdLine> ToInt(string ob) { yield return ob; }
		[Cast("i64")]
		public static IEnumerable<StdLine> ToLong(string ob) { yield return ob; }
	}
}
