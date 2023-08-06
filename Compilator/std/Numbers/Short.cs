namespace SafeC
{
	[Class("i16", "short", null, new string[] { "INumber" })]
	public class Short
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%i\", {ob}";
		}
	}
}
