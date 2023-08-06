namespace SafeC
{
	[GenericClass("OwnTyped", null, new string[0],
		new string[] {
			 "T"
		})]
	public class OwnTyped
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
			yield return new StdLine($"Typed${gen["T"].Name} this", func =>
			{
				func.Objects.Add("this",
					new Object(
						new Type(
							IClass.IsClass(Compiler.Instance!.GetClass($"Typed<{gen["T"].Name}>", gen)), true, false, false, false, true, false),
					"this"));
			});
			yield return "this.ptr = ptr";
			yield return $"this.type = {Compiler.Instance!.Classes.Name}.{gen["V"].Name}";
			yield return "return this";
		}
	}

	[GenericClass("Typed", null, new string[0],
		new string[] {
			 "T"
		})]
	public class Typed
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
			yield return new StdLine($"Typed${gen["T"].Name} this", func =>
			{
				func.Objects.Add("this",
					new Object(
						new Type(
							IClass.IsClass(Compiler.Instance!.GetClass($"Typed<{gen["T"].Name}>", gen)), true, false, false, false, true, false),
					"this"));
			});
			yield return "this.ptr = ptr";
			yield return $"this.type = {Compiler.Instance!.Classes.Name}.{gen["V"].Name}";
			yield return "return this";
		}
	}
}
