using System.Diagnostics.CodeAnalysis;

namespace RedRust
{
	internal interface Compilable
	{
		public void Compile();
	}

	internal abstract class Container : Definition
	{
		protected Container(string name, string fullName) : base(name, fullName)
		{
		}

		public new virtual Definition GetTypeDef(string name)
			=> Definition.GetTypeDef(name);

		public new virtual Interface GetInterface(string name)
			=> Definition.GetInterface(name);

		public new virtual Class GetClass(string name)
			=> Definition.GetClass(name);

		public new virtual Func? TryGetFunc(Class? of, string name)
			=> Definition.TryGetFunc(of, name);

		public new virtual Func GetFunc(Class? of, string name)
			=> Definition.GetFunc(of, name);
	}

	internal abstract class Definition : Compilable
	{
		public const char InterfaceSep = '+';
		public const char ClassSep = '-';
		public const char VarSep = ':';
		public const char FuncSep = '&';

		//Names = [a-zA-Z]{1}[a-zA-Z0-9]*;
		//Params = ({poss})(, \1)*;

		[StringSyntax(StringSyntaxAttribute.Regex)]
		public const string ClassNames = "(?'cl'[a-zA-Z]{1}[a-zA-Z0-9]*(<(?&cl)(, (?&cl))*>){0,1})";

		[StringSyntax(StringSyntaxAttribute.Regex)]
		public const string FuncNames = "(?'fn'[a-zA-Z]{1}[a-zA-Z0-9]*(<(?&fn)(, (?&fn))*>){0,1})";
		[StringSyntax(StringSyntaxAttribute.Regex)]
		public const string FuncParams = $"((?'prm'((ref )|(own )|(cp )){{1}}{ClassNames}\\?{{0,1}} [a-zA-Z]{{1}}[a-zA-Z0-9]*)(, (?&prm))*){{0,1}}";

		public static Dictionary<string, Definition> Definitions = new();

		public static void AddDef(string name, Definition def)
		{
			if (!Definitions.TryAdd(name, def))
				throw new Exception("duplicate name");
		}

		public static Definition GetTypeDef(string name)
		{
			Definition? def = null;
			if (Definitions.TryGetValue($"{InterfaceSep}{name}", out def) && def is not null && def is Interface)
				return def;
			if (Definitions.TryGetValue($"{ClassSep}{name}", out def) && def is not null && def is Class)
				return def;
			throw new Exception("interface or class name not found");
		}

		public static Interface GetInterface(string name)
		{
			if (!Definitions.TryGetValue($"{InterfaceSep}{name}", out var def) || def is null)
				throw new Exception("interface name not found");
			if (def is not Interface c)
				throw new Exception("def is not interface");
			return c;
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

		static Definition()
		{
			new Integer();
		}
	}

	internal class Integer : Class
	{
		internal Integer()
			: base("i32", null) { }

		public override void Compile() { }
	}
}
