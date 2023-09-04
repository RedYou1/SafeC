namespace SafeC
{
	[Class("f32", "float", null, new string[] { "INumber" })]
	internal class Float
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		public static void ApplyMeta(Class c, Type t)
		{
			t.MetaData.Add(new RangeMetaData<float>(
				float.MinValue, float.MaxValue
			));
		}

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%f\", {ob}";
		}

		[Cast("f64")]
		public static IEnumerable<StdLine> ToDouble(string ob) { yield return ob; }
	}
}
