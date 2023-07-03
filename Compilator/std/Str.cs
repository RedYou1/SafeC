using System.Diagnostics.CodeAnalysis;

namespace RedRust
{
	public class Str : Class
	{
		[SetsRequiredMembers]
		public Str() : base(null, Array.Empty<Class>(), true)
		{
			Name = "char*";
		}

		public void Init()
		{
			Funcs.Add("StartsWith", new Func(new(Program.BOOL, true, false, false, false, false), new (Type Type, string Name)[] { new(new(Program.STR, false, true, false, false, false), "this"), new(new(Program.STR, false, true, false, false, false), "s") }) { Name = "StartsWith" });
		}
	}
}
