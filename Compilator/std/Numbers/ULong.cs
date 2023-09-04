namespace SafeC
{
	[Class("u64", "unsigned long", null, new string[] { "INumber" })]
	internal class ULong
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		public static void ApplyMeta(Class c, Type t)
		{
			t.MetaData.Add(new RangeMetaData<ulong>(
				ulong.MinValue, ulong.MaxValue
			));
		}

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%lu\", {ob}";
		}
	}
}
