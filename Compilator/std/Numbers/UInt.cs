namespace RedRust
{
	[Class("u32", "unsigned int", null, new string[] { "INumber" })]
	public class UInt
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"\"%u\", {ob}";
		}
	}
}
