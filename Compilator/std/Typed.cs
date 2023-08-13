namespace SafeC
{
	[GenericClass("OwnTyped", null, new string[0],
		new string[] {
			 "T"
		})]
	internal class OwnTyped
	{
		public static IEnumerable<StdLine> Variables(Dictionary<string, Class> gen)
		{
			yield return $"&dyn {gen["T"].Name} ptr";
			yield return $"{Compiler.Instance!.Classes.Name} type";
		}

		[GenericConstructor(new string[] { "V" }, new string[] {
			"&V", "ptr"
		})]
		public static IEnumerable<StdLine> Constructor(Dictionary<string, Class> gen)
		{
			yield return $"Typed<{gen["T"].Name}> this";
			yield return "this.ptr = ptr";
			yield return $"this.type = {Compiler.Instance!.Classes.Name}.{gen["V"].Name}";
			yield return "return this";
		}
	}

	[GenericClass("Typed", null, new string[0],
		new string[] {
			 "T"
		})]
	internal class Typed
	{
		public static IEnumerable<StdLine> Variables(Dictionary<string, Class> gen)
		{
			yield return $"*dyn {gen["T"].Name} ptr";
			yield return $"{Compiler.Instance!.Classes.Name} type";
		}

		[GenericConstructor(new string[] { "V" }, new string[] {
			"*V", "ptr"
		})]
		public static IEnumerable<StdLine> Constructor(Dictionary<string, Class> gen)
		{
			yield return $"Typed<{gen["T"].Name}> this";
			yield return "this.ptr = ptr";
			yield return $"this.type = {Compiler.Instance!.Classes.Name}.{gen["V"].Name}";
			yield return "return this";
		}
	}
}
