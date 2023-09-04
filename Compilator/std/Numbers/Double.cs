namespace SafeC
{
	[Class("f64", "float", null, new string[] { "INumber" })]
	internal class Double
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		public static void ApplyMeta(Class c, Type t)
		{
			t.MetaData.Add(new RangeMetaData<double>(
				double.MinValue, double.MaxValue
			));
		}

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%lf\", {ob}";
		}
	}
}
