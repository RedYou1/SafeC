namespace SafeC
{
	[Class("u8", "unsigned char", null, new string[] { "INumber" })]
	internal class UByte
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		public static void ApplyMeta(Class c, Type t)
		{
			t.MetaData.Add(new RangeMetaData<byte>(
				byte.MinValue, byte.MaxValue
			));
		}

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%u\", {ob}";
		}

		[Cast("i16")]
		public static IEnumerable<StdLine> ToShort(string ob) { yield return ob; }
		[Cast("i32")]
		public static IEnumerable<StdLine> ToInt(string ob) { yield return ob; }
		[Cast("i64")]
		public static IEnumerable<StdLine> ToLong(string ob) { yield return ob; }
		[Cast("u16")]
		public static IEnumerable<StdLine> ToUShort(string ob) { yield return ob; }
		[Cast("u32")]
		public static IEnumerable<StdLine> ToUInt(string ob) { yield return ob; }
		[Cast("u64")]
		public static IEnumerable<StdLine> ToULong(string ob) { yield return ob; }
	}
}
