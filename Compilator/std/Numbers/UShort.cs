namespace RedRust
{
	[Class("u16", "unsigned short", null, new string[] { "INumber" })]
	public class UShort
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"\"%u\", {ob}";
		}
	}
}
