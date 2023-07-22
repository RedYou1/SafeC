namespace RedRust
{
	[Class("bool", "char")]
	public class Bool
	{
		public static IEnumerable<string> Variables()
		{
			yield break;
		}

		[Cast("str")]
		public static IEnumerable<string> ToStr(string ob)
		{
			yield return $"{ob} ? \"True\" : \"False\"";
		}
	}
}
