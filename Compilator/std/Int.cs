namespace RedRust
{
	[Class("i32", "int")]
	public class Int
	{
		public static IEnumerable<string> Variables()
		{
			yield break;
		}

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"\"%i\", {ob}";
		}
	}
}
