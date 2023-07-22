
namespace RedRust
{
	[Class("string", null)]
	public class String
	{
		public static IEnumerable<string> Variables()
		{
			yield return "i32 ptr";
			yield return "i32 len";
		}
	}
}
