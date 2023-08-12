namespace SafeC
{
	[Class("f64", "float", null, new string[] { "INumber" })]
	internal class Double
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%lf\", {ob}";
		}
	}
}
