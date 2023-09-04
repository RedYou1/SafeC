namespace SafeC
{
	[Class("i32", "int", null, new string[] { "INumber" })]
	internal class Int
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		public static void ApplyMeta(Class c, Type t)
		{
			t.MetaData.Add(new RangeMetaData<int>(
				int.MinValue, int.MaxValue
			));
		}

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%i\", {ob}";
		}

		[Cast("i64")]
		public static IEnumerable<StdLine> ToLong(string ob) { yield return ob; }
	}
}
