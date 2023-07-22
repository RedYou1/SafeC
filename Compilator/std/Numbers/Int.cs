namespace RedRust
{
	[Class("i32", "int", null, new string[] { "INumber" })]
	public class Int
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"\"%i\", {ob}";
		}
	}
}
