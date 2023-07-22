namespace RedRust
{
	[Class("i16", "short", null, new string[] { "INumber" })]
	public class Short
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"\"%i\", {ob}";
		}
	}
}
