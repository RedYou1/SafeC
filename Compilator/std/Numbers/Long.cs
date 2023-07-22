namespace RedRust
{
	[Class("i64", "long", null, new string[] { "INumber" })]
	public class Long
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"\"%ld\", {ob}";
		}
	}
}
