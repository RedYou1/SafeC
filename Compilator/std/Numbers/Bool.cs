namespace SafeC
{
	[Class("bool", "char", null, new string[] { "INumber" })]
	internal class Bool
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		public static void ApplyMeta(Class c, Type t)
		{
			t.MetaData.Add(new RangeMetaData<byte>(
				0, 1
			));
		}

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"{ob} ? \"True\" : \"False\"";
		}

		[Cast("u8")]
		public static IEnumerable<StdLine> ToUByte(string ob) { yield return ob; }
		[Cast("u16")]
		public static IEnumerable<StdLine> ToUShort(string ob) { yield return ob; }
		[Cast("u32")]
		public static IEnumerable<StdLine> ToUInt(string ob) { yield return ob; }
		[Cast("u64")]
		public static IEnumerable<StdLine> ToULong(string ob) { yield return ob; }
		[Cast("i8")]
		public static IEnumerable<StdLine> ToByte(string ob) { yield return ob; }
		[Cast("i16")]
		public static IEnumerable<StdLine> ToShort(string ob) { yield return ob; }
		[Cast("i32")]
		public static IEnumerable<StdLine> ToInt(string ob) { yield return ob; }
		[Cast("i64")]
		public static IEnumerable<StdLine> ToLong(string ob) { yield return ob; }
	}
}
