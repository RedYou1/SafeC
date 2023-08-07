namespace SafeC
{
	[Class("bool", "char", null, new string[] { "INumber" })]
	internal class Bool
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"{ob} ? \"True\" : \"False\"";
		}
	}
}
