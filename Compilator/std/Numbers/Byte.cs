namespace RedRust
{
	[Class("i8", "char", null, new string[] { "INumber" })]
	public class Byte
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"\"%i\", {ob}";
		}
	}
}
