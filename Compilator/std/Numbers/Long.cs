namespace SafeC
{
	[Class("i64", "long", null, new string[] { "INumber" })]
	internal class Long
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%ld\", {ob}";
		}
	}
}
