namespace SafeC
{
	[Class("u16", "unsigned short", null, new string[] { "INumber" })]
	internal class UShort
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%u\", {ob}";
		}

		[Cast("u32")]
		public static IEnumerable<StdLine> ToUInt(string ob) { yield return ob; }
		[Cast("u64")]
		public static IEnumerable<StdLine> ToULong(string ob) { yield return ob; }
	}
}
