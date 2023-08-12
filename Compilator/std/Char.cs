
namespace SafeC
{
	[Class("char", "char", null, new string[0])]
	internal class Char
	{
		public static IEnumerable<StdLine> Variables() { return Enumerable.Empty<StdLine>(); }

		[Cast("str")]
		public static IEnumerable<StdLine> ToStr(string ob)
		{
			yield return $"\"%c\", {ob}";
		}
	}
}
