namespace RedRust
{
	internal interface Compilable
	{
		public void Compile();
	}

	internal abstract class Definition : Compilable
	{
		public const char ClassSep = '.';
		public const char VarSep = ':';
		public const char FuncSep = ',';

		public static Dictionary<string, Definition> Definitions = new();

		public static void AddDef(string name, Definition def)
		{
			if (!Definitions.TryAdd(name, def))
				throw new Exception("duplicate name");
		}

		public static Class GetClass(string name)
		{
			if (!Definitions.TryGetValue($"{ClassSep}{name}", out var def) || def is null)
				throw new Exception("class name not found");
			if (def is not Class c)
				throw new Exception("def is not class");
			return c;
		}

		public static Func? TryGetFunc(Class? of, string name)
		{
			if (!Definitions.TryGetValue($"{(of is null ? "" : $"{ClassSep}{of.FullName}")}{FuncSep}{name}", out var def) || def is null)
				return null;
			if (def is not Func f)
				return null;
			return f;
		}

		public static Func GetFunc(Class? of, string name)
		{
			if (!Definitions.TryGetValue($"{(of is null ? "" : $"{ClassSep}{of.FullName}")}{FuncSep}{name}", out var def) || def is null)
				throw new Exception("class name not found");
			if (def is not Func f)
				throw new Exception("def is not class");
			return f;
		}


		public readonly string Name;

		public readonly string FullName;

		public Definition(string name, string fullName)
		{
			Name = name;
			FullName = fullName;
		}

		public abstract void Compile();
	}

	internal class Integer : Class
	{
		static Integer()
		{
			new Integer();
		}

		private Integer()
			: base("int", null) { }

		public override void Compile() { }
	}
}
