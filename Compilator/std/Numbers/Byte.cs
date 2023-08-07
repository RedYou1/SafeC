namespace SafeC
{
	[Class("i8", "char", null, new string[] { "INumber" })]
	internal class Byte
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%i\", {ob}";
		}
	}
}
