namespace SafeC
{
	[Class("u32", "unsigned int", null, new string[] { "INumber" })]
	internal class UInt
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		public static void ApplyMeta(Class c, Type t)
		{
			t.MetaData.Add(new RangeMetaData<uint>(
				uint.MinValue, uint.MaxValue
			));
		}

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%u\", {ob}";
		}

		[Cast("i64")]
		public static IEnumerable<StdLine> ToLong(string ob) { yield return ob; }
		[Cast("u64")]
		public static IEnumerable<StdLine> ToULong(string ob) { yield return ob; }
	}
}
