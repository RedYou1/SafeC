namespace RedRust
{
	[Class("f32", "float", null, new string[] { "INumber" })]
	internal class Float
	{
		public static IEnumerable<string> Variables() { return Enumerable.Empty<string>(); }

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"\"%f\", {ob}";
		}
	}
}
