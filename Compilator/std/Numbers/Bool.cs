namespace RedRust
{
	[Class("bool", "char", null, new string[] { "INumber" })]
	public class Bool
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"{ob} ? \"True\" : \"False\"";
		}
	}
}
