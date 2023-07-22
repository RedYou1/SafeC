namespace RedRust
{
	[Class("u64", "unsigned long", null, new string[] { "INumber" })]
	public class ULong
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"\"%lu\", {ob}";
		}
	}
}
