namespace SafeC
{
	[Class("i8", "char", null, new string[] { "INumber" })]
	internal class Byte
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%i\", {ob}";
		}

		[Cast("i16")]
		public static IEnumerable<StdLine> ToShort(string ob) { yield return ob; }
		[Cast("i32")]
		public static IEnumerable<StdLine> ToInt(string ob) { yield return ob; }
		[Cast("i64")]
		public static IEnumerable<StdLine> ToLong(string ob) { yield return ob; }
	}
}
