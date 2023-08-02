namespace RedRust
{
	[Class("u8", "unsigned char", null, new string[] { "INumber" })]
	public class UByte
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%u\", {ob}";
		}
	}
}
