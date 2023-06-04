namespace RedRust
{
	internal static class Enxtends
	{
		public static string AsString(this IEnumerable<char> a)
		{
			return string.Concat(a);
		}
	}
}
