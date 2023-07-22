namespace RedRust
{
	[Class("u8", "unsigned char", null, new string[] { "INumber" })]
	public class UByte
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"\"%u\", {ob}";
		}
	}
}
