namespace SafeC
{
	[Class("u8", "unsigned char", null, new string[] { "INumber" })]
	internal class UByte
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%u\", {ob}";
		}

		[Cast("u16")]
		public static IEnumerable<StdLine> ToUShort(string ob) { yield return ob; }
		[Cast("u32")]
		public static IEnumerable<StdLine> ToUInt(string ob) { yield return ob; }
		[Cast("u64")]
		public static IEnumerable<StdLine> ToULong(string ob) { yield return ob; }
	}
}
