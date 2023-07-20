
namespace RedRust
{
	public class Str : Class
	{
		public Str() : base("char*", null, Array.Empty<Class>(), true)
		{
		}

		public void Init()
		{
			Funcs.Add("StartsWith", new Func(new(Program.BOOL, true, false, false, false, false), new (Type Type, string Name)[] { new(new(Program.STR, false, false, false, false, false), "this"), new(new(Program.STR, false, false, false, false, false), "s") }) { Name = "StartsWith" });
		}
	}
}
