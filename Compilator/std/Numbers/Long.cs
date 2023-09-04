namespace SafeC
{
	[Class("i64", "long", null, new string[] { "INumber" })]
	internal class Long
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		public static void ApplyMeta(Class c, Type t)
		{
			t.MetaData.Add(new RangeMetaData<long>(
				long.MinValue, long.MaxValue
			));
		}

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%ld\", {ob}";
		}
	}
}
